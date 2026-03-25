using System.Collections.Concurrent;

namespace AcilEvrak.Infrastructure.Cache;

public sealed class InMemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(key, out var entry))
            return Task.FromResult<T?>(default);

        if (entry.ExpiresAt.HasValue && entry.ExpiresAt.Value < DateTime.UtcNow)
        {
            _cache.TryRemove(key, out _);
            return Task.FromResult<T?>(default);
        }

        return Task.FromResult((T?)entry.Value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var expiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : (DateTime?)null;
        _cache[key] = new CacheEntry(value, expiresAt);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    private sealed record CacheEntry(object? Value, DateTime? ExpiresAt);
}
