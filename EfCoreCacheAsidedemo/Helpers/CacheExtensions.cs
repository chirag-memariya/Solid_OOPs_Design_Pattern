using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace EfCoreCacheAsidedemo.Helpers;

public static class CacheExtensions
{
    public static async Task<T?> GetAsync<T>(this IDistributedCache cache,string key)
    {
        var json = await cache.GetStringAsync(key);
        return json == null ? default : JsonSerializer.Deserialize<T>(json);
    }

    public static async Task SetAsync<T>(this IDistributedCache cache,string key,T value,TimeSpan ttl)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };

        await cache.SetStringAsync(key,JsonSerializer.Serialize(value),
        options);
    }
}