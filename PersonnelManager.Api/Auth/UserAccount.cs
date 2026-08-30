namespace PersonnelManager.Api.Auth;

/// <summary>
/// A demo user account bound from the "Auth:Users" configuration section.
/// In a real system this would be a table of hashed credentials; here it is
/// deliberately simple config so the API is runnable out of the box.
/// </summary>
public sealed class UserAccount
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string[] Roles { get; init; } = [];
}

/// <summary>Container for the "Auth" configuration section.</summary>
public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public List<UserAccount> Users { get; init; } = [];
}
