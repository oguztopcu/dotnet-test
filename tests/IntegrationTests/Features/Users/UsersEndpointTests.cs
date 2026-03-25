using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using IntegrationTests.Fixtures;

namespace IntegrationTests.Features.Users;

[Collection("Integration")]
public sealed class UsersEndpointTests : IntegrationTestBase
{
    public UsersEndpointTests(PostgreSqlFixture fixture) : base(fixture) { }

    [Fact]
    public async Task CreateUser_ValidRequest_ReturnsCreated()
    {
        SetNewIdempotencyKey();
        var request = new
        {
            Email = $"create-{Guid.CreateVersion7():N}@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Role = "User"
        };

        var response = await Client.PostAsJsonAsync("/api/v1/users", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadFromJsonAsync<JsonDocument>();
        json!.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        json.RootElement.GetProperty("data").GetProperty("email").GetString().Should().Be(request.Email);
    }

    [Fact]
    public async Task CreateUser_DuplicateEmail_ReturnsConflict()
    {
        var user = await CreateTestUserAsync();

        SetNewIdempotencyKey();
        var request = new
        {
            Email = user.Email,
            Password = "Password123!",
            FirstName = "Jane",
            LastName = "Doe",
            Role = "User"
        };

        var response = await Client.PostAsJsonAsync("/api/v1/users", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateUser_InvalidEmail_ReturnsBadRequest()
    {
        SetNewIdempotencyKey();
        var request = new
        {
            Email = "invalid-email",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Role = "User"
        };

        var response = await Client.PostAsJsonAsync("/api/v1/users", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserByUuid_ExistingUser_ReturnsOk()
    {
        var user = await CreateTestUserAsync();

        var response = await Client.GetAsync($"/api/v1/users/{user.Uuid}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonDocument>();
        json!.RootElement.GetProperty("data").GetProperty("uuid").GetGuid().Should().Be(user.Uuid);
    }

    [Fact]
    public async Task GetUserByUuid_NonExistingUser_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"/api/v1/users/{Guid.CreateVersion7()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUsers_ReturnsPagedResponse()
    {
        await CreateTestUserAsync();

        var response = await Client.GetAsync("/api/v1/users?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonDocument>();
        json!.RootElement.GetProperty("data").GetProperty("items").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateUser_ExistingUser_ReturnsOk()
    {
        var user = await CreateTestUserAsync();

        SetNewIdempotencyKey();
        var request = new
        {
            FirstName = "Updated",
            LastName = "Name",
            Role = "Admin",
            IsActive = true
        };

        var response = await Client.PutAsJsonAsync($"/api/v1/users/{user.Uuid}", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteUser_ExistingUser_ReturnsOk()
    {
        var user = await CreateTestUserAsync();

        SetNewIdempotencyKey();
        var response = await Client.DeleteAsync($"/api/v1/users/{user.Uuid}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await Client.GetAsync($"/api/v1/users/{user.Uuid}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateUser_MissingIdempotencyKey_ReturnsBadRequest()
    {
        Client.DefaultRequestHeaders.Remove("X-Idempotency-Key");
        var request = new
        {
            Email = $"no-key-{Guid.CreateVersion7():N}@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Role = "User"
        };

        var response = await Client.PostAsJsonAsync("/api/v1/users", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
