using System.Net;
using System.Net.Http.Json;
using PersonnelManager.Api.Contracts;

namespace PersonnelManager.Api.Tests;

/// <summary>Authentication and authorization behaviour at the HTTP boundary.</summary>
public sealed class AuthApiTests
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsBearerToken()
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", "admin123"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(ApiFactory.Json);
        Assert.False(string.IsNullOrWhiteSpace(token!.AccessToken));
        Assert.Equal("Bearer", token.TokenType);
        Assert.True(token.ExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithBadPassword_Returns401()
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", "wrong"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithMissingFields_Returns400ValidationProblem()
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("", ""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Personnel_WithoutToken_Returns401()
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/personnel");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
