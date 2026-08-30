using System.Security.Claims;
using System.Text.Json;

namespace PersonnelManager.Web.ApiClient;

/// <summary>
/// Minimal read-only JWT payload decoder. The token is issued and *verified* by the API; the web
/// app only needs to read its claims (username, roles) to build the local auth cookie, so a full
/// signature-validating handler would be overkill here.
/// </summary>
public static class JwtReader
{
    /// <summary>Extract Name and Role claims from a JWT's payload.</summary>
    public static IEnumerable<Claim> ReadClaims(string token)
    {
        var parts = token.Split('.');
        if (parts.Length < 2)
            yield break;

        using var doc = JsonDocument.Parse(Base64UrlDecode(parts[1]));
        var root = doc.RootElement;

        foreach (var name in new[] { "name", "unique_name", "sub" })
            if (root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
            {
                yield return new Claim(ClaimTypes.Name, value.GetString()!);
                break;
            }

        // Role claims may arrive as "role", "roles", or the full ClaimTypes.Role URI; and as a
        // single string or an array. Normalise them all to ClaimTypes.Role.
        foreach (var key in new[] { "role", "roles", ClaimTypes.Role })
        {
            if (!root.TryGetProperty(key, out var roleElement))
                continue;

            if (roleElement.ValueKind == JsonValueKind.String)
                yield return new Claim(ClaimTypes.Role, roleElement.GetString()!);
            else if (roleElement.ValueKind == JsonValueKind.Array)
                foreach (var item in roleElement.EnumerateArray())
                    if (item.ValueKind == JsonValueKind.String)
                        yield return new Claim(ClaimTypes.Role, item.GetString()!);
        }
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        s = (s.Length % 4) switch
        {
            2 => s + "==",
            3 => s + "=",
            _ => s,
        };
        return Convert.FromBase64String(s);
    }
}
