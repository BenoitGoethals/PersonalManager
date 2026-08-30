using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace PersonnelManager.Api.Auth;

/// <summary>Issues signed JWT access tokens for authenticated users.</summary>
public interface ITokenService
{
    /// <summary>Validate credentials and, on success, return a signed JWT and its expiry.</summary>
    (string Token, DateTime ExpiresAtUtc)? Issue(string username, string password);
}

/// <summary>
/// Validates credentials against the configured demo users and mints an HS256 JWT
/// carrying the user's name and role claims. The signing key comes from <see cref="JwtOptions"/>.
/// </summary>
public sealed class TokenService(IOptions<JwtOptions> jwtOptions, IOptions<AuthOptions> authOptions)
    : ITokenService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;
    private readonly AuthOptions _auth = authOptions.Value;

    public (string Token, DateTime ExpiresAtUtc)? Issue(string username, string password)
    {
        var user = _auth.Users.FirstOrDefault(u =>
            string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase) &&
            u.Password == password);

        if (user is null)
            return null;

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, user.Username),
        };
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
