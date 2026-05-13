using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace ProductGrpcServer.Helpers;

public static class CacheExtensions
{
    public static async Task<T?> GetAsync<T>(
        this IDistributedCache cache, string key)
    {
        var json = await cache.GetStringAsync(key);
        return json == null ? default : JsonSerializer.Deserialize<T>(json);
    }

    public static async Task SetAsync<T>(
        this IDistributedCache cache, string key, T value, TimeSpan ttl)
    {
        await cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(value),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            });
    }
}
