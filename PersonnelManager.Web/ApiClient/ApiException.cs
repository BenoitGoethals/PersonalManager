using System.Net;

namespace PersonnelManager.Web.ApiClient;

/// <summary>
/// Raised when the API returns a non-success status. Carries the status code and, for 400
/// validation responses, the per-field errors so pages can surface them in ModelState.
/// </summary>
public sealed class ApiException(
    HttpStatusCode statusCode, string message, IReadOnlyDictionary<string, string[]>? errors = null)
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public IReadOnlyDictionary<string, string[]> Errors { get; } =
        errors ?? new Dictionary<string, string[]>();

    public bool IsValidationError => StatusCode == HttpStatusCode.BadRequest && Errors.Count > 0;
    public bool IsUnauthorized => StatusCode is HttpStatusCode.Unauthorized;
}
