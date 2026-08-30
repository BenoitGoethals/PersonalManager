using System.Net;
using System.Net.Http.Json;
using PersonnelManager.Api.Contracts;
using PersonnelManager.Domain;

namespace PersonnelManager.Api.Tests;

/// <summary>End-to-end CRUD + status/search/backup and role enforcement over HTTP.</summary>
public sealed class PersonnelApiTests
{
    private static CreatePersonRequest SampleCreate(string name = "Ada", string surname = "Lovelace") =>
        new(name, surname, "London", "+44 100", EmploymentStatus.Active);

    private static async Task<PersonalDto> CreatePersonAsync(HttpClient client, CreatePersonRequest request)
    {
        var response = await client.PostAsJsonAsync("/api/personnel", request, ApiFactory.Json);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<PersonalDto>(ApiFactory.Json))!;
    }

    [Fact]
    public async Task Create_ThenGetById_ReturnsCreatedPerson()
    {
        using var factory = new ApiFactory();
        var client = await factory.CreateAuthenticatedClientAsync("admin", "admin123");

        var created = await CreatePersonAsync(client, SampleCreate());

        Assert.NotEqual(Guid.Empty, created.Id);
        var fetched = await client.GetFromJsonAsync<PersonalDto>($"/api/personnel/{created.Id}", ApiFactory.Json);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal("Ada", fetched.Name);
        Assert.Equal(EmploymentStatus.Active, fetched.Status);
    }

    [Fact]
    public async Task Create_SetsLocationHeaderToTheNewResource()
    {
        using var factory = new ApiFactory();
        var client = await factory.CreateAuthenticatedClientAsync("user", "user123");

        var response = await client.PostAsJsonAsync("/api/personnel", SampleCreate(), ApiFactory.Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Create_WithNoNameOrSurname_Returns400ValidationProblem()
    {
        using var factory = new ApiFactory();
        var client = await factory.CreateAuthenticatedClientAsync("admin", "admin123");

        var response = await client.PostAsJsonAsync(
            "/api/personnel", new CreatePersonRequest(null, null, "London", "123", EmploymentStatus.Active), ApiFactory.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>(ApiFactory.Json);
        Assert.Equal("One or more validation errors occurred.", problem!.Title);
    }

    [Fact]
    public async Task GetById_UnknownId_Returns404()
    {
        using var factory = new ApiFactory();
        var client = await factory.CreateAuthenticatedClientAsync("admin", "admin123");

        var response = await client.GetAsync($"/api/personnel/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ChangesFields()
    {
        using var factory = new ApiFactory();
        var client = await factory.CreateAuthenticatedClientAsync("admin", "admin123");
        var created = await CreatePersonAsync(client, SampleCreate());

        var response = await client.PutAsJsonAsync(
            $"/api/personnel/{created.Id}",
            new UpdatePersonRequest("Grace", "Hopper", "New York", "+1 555", EmploymentStatus.Active),
            ApiFactory.Json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<PersonalDto>(ApiFactory.Json);
        Assert.Equal("Grace", updated!.Name);
        Assert.Equal("Hopper", updated.Surname);
    }

    [Fact]
    public async Task ChangeStatus_UpdatesOnlyStatus()
    {
        using var factory = new ApiFactory();
        var client = await factory.CreateAuthenticatedClientAsync("admin", "admin123");
        var created = await CreatePersonAsync(client, SampleCreate());

        var response = await client.PatchAsJsonAsync(
            $"/api/personnel/{created.Id}/status", new ChangeStatusRequest(EmploymentStatus.OnLeave), ApiFactory.Json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var patched = await response.Content.ReadFromJsonAsync<PersonalDto>(ApiFactory.Json);
        Assert.Equal(EmploymentStatus.OnLeave, patched!.Status);
        Assert.Equal("Ada", patched.Name); // other fields untouched
    }

    [Fact]
    public async Task GetAll_FiltersByStatusAndName()
    {
        using var factory = new ApiFactory();
        var client = await factory.CreateAuthenticatedClientAsync("admin", "admin123");
        var ada = await CreatePersonAsync(client, SampleCreate("Ada", "Lovelace"));
        await CreatePersonAsync(client, SampleCreate("Grace", "Hopper"));
        await client.PatchAsJsonAsync(
            $"/api/personnel/{ada.Id}/status", new ChangeStatusRequest(EmploymentStatus.OnLeave), ApiFactory.Json);

        var matches = await client.GetFromJsonAsync<List<PersonalDto>>(
            "/api/personnel?status=OnLeave&name=ada", ApiFactory.Json);

        Assert.Single(matches!);
        Assert.Equal(ada.Id, matches![0].Id);
    }

    [Fact]
    public async Task Delete_AsUser_Returns403()
    {
        using var factory = new ApiFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin", "admin123");
        var created = await CreatePersonAsync(admin, SampleCreate());

        var user = await factory.CreateAuthenticatedClientAsync("user", "user123");
        var response = await user.DeleteAsync($"/api/personnel/{created.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AsAdmin_Returns204_ThenGetIs404()
    {
        using var factory = new ApiFactory();
        var client = await factory.CreateAuthenticatedClientAsync("admin", "admin123");
        var created = await CreatePersonAsync(client, SampleCreate());

        var delete = await client.DeleteAsync($"/api/personnel/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var get = await client.GetAsync($"/api/personnel/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Backup_AsUser_Returns403_AsAdmin_Returns202()
    {
        using var factory = new ApiFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin", "admin123");
        await CreatePersonAsync(admin, SampleCreate());

        var user = await factory.CreateAuthenticatedClientAsync("user", "user123");
        var forbidden = await user.PostAsync("/api/personnel/backup", content: null);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        var accepted = await admin.PostAsync("/api/personnel/backup", content: null);
        Assert.Equal(HttpStatusCode.Accepted, accepted.StatusCode);
    }

    [Fact]
    public async Task Health_ReportsHealthy()
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", body);
    }

    /// <summary>Minimal shape for asserting on RFC 7807 validation responses.</summary>
    private sealed record ValidationProblemResponse(string? Title, int? Status);
}
