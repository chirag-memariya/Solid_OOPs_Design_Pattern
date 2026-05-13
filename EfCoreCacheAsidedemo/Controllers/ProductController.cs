using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

using EfCoreCacheAsidedemo.Data;
using EfCoreCacheAsidedemo.Helpers;
using EfCoreCacheAsidedemo.Models;

namespace EfCoreCacheAsidedemo.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IDistributedCache _cache;
    public ProductController(AppDbContext db,IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var cacheKey = $"product_{id}";
        //1.cache lookup
        var cached =await _cache.GetAsync<Product>(cacheKey);
        if (cached != null)
        {
            return Ok(new {source = "cache",data=cached});
        }

        //2.DB lookup 
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p=>p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        //3.Populate cache
        await _cache.SetAsync(cacheKey,product,TimeSpan.FromMinutes(5));
        return Ok(new {source="database",data=product});
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        //optional: cache immediately 
        var cacheKey = $"product_{product.Id}";
        await _cache.SetAsync(cacheKey,product,TimeSpan.FromMinutes(5));
        return CreatedAtAction(nameof(Get),new {id = product.Id},product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id,Product input)
    {
        var product = await _db.Products.FindAsync(id);
        if(product == null)
        {
            return NotFound();
        }
        product.Name = input.Name;
        product.Price = input.Price;

        //4. cache invalidation
        await _cache.RemoveAsync($"product_{id}");
        return NoContent();
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        await _cache.RemoveAsync($"product_{id}");

        return NoContent();
    }
}