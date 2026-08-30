using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PersonnelManager.Web.ApiClient;

/// <summary>The operations the web app needs from PersonnelManager.Api.</summary>
public interface IPersonnelApiClient
{
    Task<TokenResponse> LoginAsync(string username, string password, CancellationToken ct = default);
    Task<IReadOnlyList<PersonView>> GetAllAsync(EmploymentStatus? status, string? name, CancellationToken ct = default);
    Task<PersonView?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PersonView> CreateAsync(PersonInput input, CancellationToken ct = default);
    Task<PersonView> UpdateAsync(Guid id, PersonInput input, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task BackupAsync(CancellationToken ct = default);
}

/// <summary>
/// Typed HttpClient over the REST API. The <see cref="BearerTokenHandler"/> injects the JWT;
/// this class handles (de)serialization and turns non-success responses into <see cref="ApiException"/>.
/// </summary>
public sealed class PersonnelApiClient(HttpClient http) : IPersonnelApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public async Task<TokenResponse> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("/api/auth/login", new { username, password }, Json, ct);
        await EnsureSuccessAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<TokenResponse>(Json, ct))!;
    }

    public async Task<IReadOnlyList<PersonView>> GetAllAsync(
        EmploymentStatus? status, string? name, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (status is not null)
            query.Add($"status={status}");
        if (!string.IsNullOrWhiteSpace(name))
            query.Add($"name={Uri.EscapeDataString(name)}");
        var url = "/api/personnel" + (query.Count > 0 ? "?" + string.Join("&", query) : "");

        var response = await http.GetAsync(url, ct);
        await EnsureSuccessAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<List<PersonView>>(Json, ct)) ?? [];
    }

    public async Task<PersonView?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"/api/personnel/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<PersonView>(Json, ct);
    }

    public async Task<PersonView> CreateAsync(PersonInput input, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("/api/personnel", ToBody(input), Json, ct);
        await EnsureSuccessAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<PersonView>(Json, ct))!;
    }

    public async Task<PersonView> UpdateAsync(Guid id, PersonInput input, CancellationToken ct = default)
    {
        var response = await http.PutAsJsonAsync($"/api/personnel/{id}", ToBody(input), Json, ct);
        await EnsureSuccessAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<PersonView>(Json, ct))!;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var response = await http.DeleteAsync($"/api/personnel/{id}", ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task BackupAsync(CancellationToken ct = default)
    {
        var response = await http.PostAsync("/api/personnel/backup", content: null, ct);
        await EnsureSuccessAsync(response, ct);
    }

    private static object ToBody(PersonInput input) =>
        new { input.Name, input.Surname, input.Address, input.Phone, status = input.Status.ToString() };

    /// <summary>Throw a rich <see cref="ApiException"/> for non-success responses (parsing ProblemDetails).</summary>
    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        var title = response.ReasonPhrase ?? "Request failed.";
        IReadOnlyDictionary<string, string[]>? errors = null;

        if (response.Content.Headers.ContentType?.MediaType?.Contains("json") == true)
        {
            try
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemPayload>(Json, ct);
                if (problem is not null)
                {
                    title = problem.Title ?? title;
                    errors = problem.Errors;
                }
            }
            catch (JsonException) { /* fall back to the reason phrase */ }
        }

        throw new ApiException(response.StatusCode, title, errors);
    }

    /// <summary>Shape covering both ProblemDetails and ValidationProblemDetails responses.</summary>
    private sealed record ProblemPayload(string? Title, int? Status, Dictionary<string, string[]>? Errors);
}
