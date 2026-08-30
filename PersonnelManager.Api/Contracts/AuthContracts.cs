namespace PersonnelManager.Api.Contracts;

/// <summary>Credentials posted to <c>POST /api/auth/login</c>.</summary>
public record LoginRequest(string Username, string Password);

/// <summary>The bearer token returned on a successful login.</summary>
public record TokenResponse(string AccessToken, DateTime ExpiresAtUtc, string TokenType = "Bearer");
