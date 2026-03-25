using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.Builders;

namespace IntegrationTests.Fixtures;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private CustomWebApplicationFactory _factory = default!;
    protected HttpClient Client { get; private set; } = default!;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected IntegrationTestBase(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _factory = new CustomWebApplicationFactory(_fixture.ConnectionString);
        Client = _factory.CreateClient();
        Client.DefaultRequestHeaders.Add("X-Idempotency-Key", Guid.CreateVersion7().ToString());

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
    }

    protected void SetNewIdempotencyKey()
    {
        Client.DefaultRequestHeaders.Remove("X-Idempotency-Key");
        Client.DefaultRequestHeaders.Add("X-Idempotency-Key", Guid.CreateVersion7().ToString());
    }

    protected void SetAuthorizationHeader(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected async Task<UserBuilder.CreatedUser> CreateTestUserAsync(string? email = null, string? password = null)
    {
        var builder = new UserBuilder()
            .WithEmail(email ?? $"test-{Guid.CreateVersion7():N}@example.com")
            .WithPassword(password ?? "TestPassword123!");

        SetNewIdempotencyKey();
        var response = await Client.PostAsJsonAsync("/api/v1/users", builder.BuildCreateCommand(), JsonOptions);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var uuid = json!.RootElement.GetProperty("data").GetProperty("uuid").GetGuid();

        return new UserBuilder.CreatedUser(uuid, builder.Email, builder.Password);
    }

    protected async Task<string> LoginAndGetTokenAsync(string email, string password)
    {
        SetNewIdempotencyKey();
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", new { email, password }, JsonOptions);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonDocument>();
        return json!.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;
    }
}
