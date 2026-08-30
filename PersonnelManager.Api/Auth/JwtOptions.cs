using System.ComponentModel.DataAnnotations;

namespace PersonnelManager.Api.Auth;

/// <summary>
/// Strongly-typed JWT settings bound from the "Jwt" configuration section.
/// Validated on startup (see Program.cs) so a misconfigured signing key fails fast
/// rather than silently issuing unverifiable tokens.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    /// <summary>Symmetric signing key. Keep this secret; use user-secrets / env vars in real deployments.</summary>
    [Required]
    [MinLength(32, ErrorMessage = "The JWT signing key must be at least 32 characters (256 bits) for HS256.")]
    public string SigningKey { get; init; } = string.Empty;

    /// <summary>How long an issued access token remains valid.</summary>
    [Range(1, 1440)]
    public int AccessTokenMinutes { get; init; } = 60;
}
