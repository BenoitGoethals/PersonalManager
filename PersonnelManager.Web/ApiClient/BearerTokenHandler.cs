using System.Net.Http.Headers;
using System.Security.Claims;

namespace PersonnelManager.Web.ApiClient;

/// <summary>
/// Attaches the signed-in user's JWT (stashed as the "access_token" claim in the auth cookie)
/// as a Bearer header on every outgoing API call. Anonymous requests (e.g. login) simply go
/// out without a token.
/// </summary>
public sealed class BearerTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    public const string AccessTokenClaim = "access_token";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = httpContextAccessor.HttpContext?.User.FindFirstValue(AccessTokenClaim);
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
