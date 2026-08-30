namespace PersonnelManager.Api.Auth;

/// <summary>
/// The role names used across the API. Centralised as constants so controller
/// [Authorize(Roles = ...)] attributes and token issuance can never drift apart.
/// </summary>
public static class Roles
{
    /// <summary>Full access, including destructive operations (delete) and backups.</summary>
    public const string Admin = "Admin";

    /// <summary>Read and write personnel, but not delete or trigger backups.</summary>
    public const string User = "User";
}
