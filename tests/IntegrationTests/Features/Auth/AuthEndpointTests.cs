using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using IntegrationTests.Fixtures;

namespace IntegrationTests.Features.Auth;

[Collection("Integration")]
public sealed class AuthEndpointTests : IntegrationTestBase
{
    public AuthEndpointTests(PostgreSqlFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Register_ValidRequest_ReturnsCreated()
    {
        SetNewIdempotencyKey();
        var request = new
        {
            Email = $"register-{Guid.CreateVersion7():N}@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };

        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadFromJsonAsync<JsonDocument>();
        json!.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        json.RootElement.GetProperty("data").GetProperty("email").GetString().Should().Be(request.Email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var email = $"dup-{Guid.CreateVersion7():N}@example.com";

        SetNewIdempotencyKey();
        await Client.PostAsJsonAsync("/api/v1/auth/register", new { Email = email, Password = "Password123!", FirstName = "A", LastName = "B" }, JsonOptions);

        SetNewIdempotencyKey();
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", new { Email = email, Password = "Password123!", FirstName = "C", LastName = "D" }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        var user = await CreateTestUserAsync();

        SetNewIdempotencyKey();
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", new { Email = user.Email, Password = user.Password }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonDocument>();
        json!.RootElement.GetProperty("data").GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        json.RootElement.GetProperty("data").GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var user = await CreateTestUserAsync();

        SetNewIdempotencyKey();
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", new { Email = user.Email, Password = "WrongPassword!" }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_ValidToken_ReturnsNewTokens()
    {
        var user = await CreateTestUserAsync();

        SetNewIdempotencyKey();
        var loginResponse = await Client.PostAsJsonAsync("/api/v1/auth/login", new { Email = user.Email, Password = user.Password }, JsonOptions);
        var loginJson = await loginResponse.Content.ReadFromJsonAsync<JsonDocument>();
        var refreshToken = loginJson!.RootElement.GetProperty("data").GetProperty("refreshToken").GetString()!;

        SetNewIdempotencyKey();
        var response = await Client.PostAsJsonAsync("/api/v1/auth/refresh", new { RefreshToken = refreshToken }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonDocument>();
        json!.RootElement.GetProperty("data").GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Refresh_InvalidToken_ReturnsUnauthorized()
    {
        SetNewIdempotencyKey();
        var response = await Client.PostAsJsonAsync("/api/v1/auth/refresh", new { RefreshToken = "invalid-token" }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ValidSession_ReturnsOk()
    {
        var user = await CreateTestUserAsync();
        var token = await LoginAndGetTokenAsync(user.Email, user.Password);
        SetAuthorizationHeader(token);

        SetNewIdempotencyKey();
        var response = await Client.PostAsync("/api/v1/auth/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSessions_Authenticated_ReturnsSessionList()
    {
        var user = await CreateTestUserAsync();
        var token = await LoginAndGetTokenAsync(user.Email, user.Password);
        SetAuthorizationHeader(token);

        var response = await Client.GetAsync("/api/v1/auth/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonDocument>();
        json!.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetSessions_Unauthenticated_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/v1/auth/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
