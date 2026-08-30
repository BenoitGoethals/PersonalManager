using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonnelManager.Api.Auth;
using PersonnelManager.Api.Contracts;

namespace PersonnelManager.Api.Controllers;

/// <summary>
/// Issues JWT access tokens. This is the only anonymous controller — everything else
/// requires a valid bearer token obtained here.
/// </summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController(ITokenService tokenService) : ControllerBase
{
    /// <summary>Exchange a username/password for a signed JWT bearer token.</summary>
    /// <response code="200">Credentials accepted; returns an access token.</response>
    /// <response code="401">Invalid username or password.</response>
    [HttpPost("login")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<TokenResponse> Login([FromBody] LoginRequest request)
    {
        var issued = tokenService.Issue(request.Username, request.Password);
        return issued is null
            ? Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid username or password.")
            : Ok(new TokenResponse(issued.Value.Token, issued.Value.ExpiresAtUtc));
    }
}
