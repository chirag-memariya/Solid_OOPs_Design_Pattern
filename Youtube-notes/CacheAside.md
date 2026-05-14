Below is the **same Cache-Aside pattern with EF Core**, but using **PostgreSQL running in Docker** instead of SQL Server.
Controller-based ASP.NET Core Web API (not Minimal API).

---

## 1. Run PostgreSQL in Docker

```bash
docker run -d \
  --name postgres-cache-demo \
  -e POSTGRES_USER=appuser \
  -e POSTGRES_PASSWORD=apppassword \
  -e POSTGRES_DB=cache_demo_db \
  -p 5432:5432 \
  postgres:16
```

Verify:

```bash
docker ps
```

---

## 2. Create Web API Project

```bash
dotnet new webapi -n EfCoreCacheAsidePostgres
cd EfCoreCacheAsidePostgres
```

---

## 3. Add Required NuGet Packages

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

---

## 4. Run Redis (for Distributed Cache)

```bash
docker run -d -p 6379:6379 --name redis redis
```

---

## 5. Create Entity

`Models/Product.cs`

```csharp
namespace EfCoreCacheAsidePostgres.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

---

## 6. Create DbContext

`Data/AppDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using EfCoreCacheAsidePostgres.Models;

namespace EfCoreCacheAsidePostgres.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
}
```

---

## 7. Configure PostgreSQL + Redis (`Program.cs`)

```csharp
using EfCoreCacheAsidePostgres.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// PostgreSQL (EF Core)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Redis Distributed Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "PostgresCache:";
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

---

## 8. Configure Connection String

`appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=cache_demo_db;Username=appuser;Password=apppassword"
  }
}
```

---

## 9. Create Database & Tables

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

PostgreSQL tables will be created automatically.

---

## 10. Cache Helper (Serialization)

`Helpers/CacheExtensions.cs`

```csharp
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace EfCoreCacheAsidePostgres.Helpers;

public static class CacheExtensions
{
    public static async Task<T?> GetAsync<T>(
        this IDistributedCache cache, string key)
    {
        var json = await cache.GetStringAsync(key);
        return json == null ? default : JsonSerializer.Deserialize<T>(json);
    }

    public static async Task SetAsync<T>(
        this IDistributedCache cache,
        string key,
        T value,
        TimeSpan ttl)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };

        await cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(value),
            options);
    }
}
```

---

## 11. Implement Cache-Aside in Controller

`Controllers/ProductController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using EfCoreCacheAsidePostgres.Data;
using EfCoreCacheAsidePostgres.Helpers;
using EfCoreCacheAsidePostgres.Models;

namespace EfCoreCacheAsidePostgres.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IDistributedCache _cache;

    public ProductController(AppDbContext db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var cacheKey = $"product_{id}";

        // 1. Cache lookup
        var cached = await _cache.GetAsync<Product>(cacheKey);
        if (cached != null)
        {
            return Ok(new { source = "cache", data = cached });
        }

        // 2. DB lookup
        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        // 3. Populate cache
        await _cache.SetAsync(cacheKey, product, TimeSpan.FromMinutes(5));

        return Ok(new { source = "database", data = product });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Product input)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        product.Name = input.Name;
        product.Price = input.Price;

        await _db.SaveChangesAsync();

        // 4. Cache invalidation
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
```

---

## 12. Request Flow (Clear)

1. Client → API
2. API → Redis
3. Cache miss → PostgreSQL (EF Core)
4. Result cached in Redis
5. Response returned

---

## 13. Why PostgreSQL + Cache-Aside Works Well

* PostgreSQL handles **strong persistence**
* Redis handles **fast reads**
* EF Core controls **cache lifecycle**
* Works perfectly in **Docker + Kubernetes**

---

## 14. Production Notes

* Use `AsNoTracking()` for cached reads
* Short TTLs (5–10 min)
* Always invalidate cache on update/delete
* Never cache large object graphs

---

If you want next:

* Docker Compose (API + Postgres + Redis)
* Cache stampede prevention
* Redis pub/sub invalidation
* Generic cache repository decorator

Tell me which one.
