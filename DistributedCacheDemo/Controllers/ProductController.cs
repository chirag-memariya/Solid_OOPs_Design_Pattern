using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using DistributedCacheDemo.Models;
using Microsoft.OpenApi.Expressions;

namespace DistributedCacheDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IDistributedCache _cache;
    public ProductController(IDistributedCache cache)
    {
        _cache = cache;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        string cacheKey = $"product_{id}";
        //1. try to get data from cache
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if(cachedData != null)
        {
            var cacheProduct = JsonSerializer.Deserialize<Product>(cachedData);
            return Ok(new {source = "cache",data = cacheProduct});
        }

        //2. Simulate DB call
        var productFromdb = new Product
        {
            Id = id,
            Name = "Keyboard",
            Price = 2500
        };

        //3.store in cache
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(1)
        };

        var serializeData = JsonSerializer.Serialize(productFromdb);
        await _cache.SetStringAsync(cacheKey,serializeData,cacheOptions);
        return Ok(new {source="database",data = productFromdb});
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveFromCache(int id)
    {
        string cacheKey = $"product_{id}";
        await _cache.RemoveAsync(cacheKey);
        return Ok("Cache cleared");
    }
}