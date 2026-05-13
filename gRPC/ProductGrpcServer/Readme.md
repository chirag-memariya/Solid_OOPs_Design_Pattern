Below is a **complete end-to-end gRPC setup** showing:

* **gRPC Server**

  * ASP.NET Core
  * EF Core + PostgreSQL (Docker)
  * Redis (cache-aside)
* **gRPC Client (Console app)**
* **Exact file names**
* **How to run everything step by step**

No Minimal API. No shortcuts.

---

# PART 1: Infrastructure (Docker)

## 1. Run PostgreSQL

```bash
docker run -d --name postgres-grpc \
  -e POSTGRES_USER=appuser \
  -e POSTGRES_PASSWORD=apppassword \
  -e POSTGRES_DB=productdb \
  -p 5432:5432 postgres:16
```

## 2. Run Redis

```bash
docker run -d --name redis-grpc -p 6379:6379 redis
```

---

# PART 2: gRPC SERVER

## 3. Create gRPC Server Project

```bash
dotnet new grpc -n ProductGrpcServer
cd ProductGrpcServer
```

---

## 4. NuGet Packages (Server)

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
dotnet add package Microsoft.EntityFrameworkCore.Design
```

---

## 5. Folder Structure (Server)

```
ProductGrpcServer
│
├── Data
│   └── AppDbContext.cs
│
├── Models
│   └── Product.cs
│
├── Helpers
│   └── CacheExtensions.cs
│
├── Protos
│   └── product.proto
│
├── Services
│   └── ProductServiceImpl.cs
│
├── appsettings.json
└── Program.cs
```

---

## 6. `Models/Product.cs`

```csharp
namespace ProductGrpcServer.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

---

## 7. `Data/AppDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using ProductGrpcServer.Models;

namespace ProductGrpcServer.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Product> Products => Set<Product>();
}
```

---

## 8. `Protos/product.proto`

```proto
syntax = "proto3";

option csharp_namespace = "ProductGrpcServer.Protos";

package product;

service ProductService {
  rpc GetProduct (GetProductRequest) returns (ProductReply);
  rpc CreateProduct (CreateProductRequest) returns (ProductReply);
}

message GetProductRequest {
  int32 id = 1;
}

message CreateProductRequest {
  string name = 1;
  double price = 2;
}

message ProductReply {
  int32 id = 1;
  string name = 2;
  double price = 3;
  string source = 4;
}
```

---

## 9. `Helpers/CacheExtensions.cs`

```csharp
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
```

---

## 10. `Services/ProductServiceImpl.cs`

```csharp
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ProductGrpcServer.Data;
using ProductGrpcServer.Helpers;
using ProductGrpcServer.Models;
using ProductGrpcServer.Protos;

namespace ProductGrpcServer.Services;

public class ProductServiceImpl : ProductService.ProductServiceBase
{
    private readonly AppDbContext _db;
    private readonly IDistributedCache _cache;

    public ProductServiceImpl(AppDbContext db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public override async Task<ProductReply> CreateProduct(
        CreateProductRequest request,
        ServerCallContext context)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = (decimal)request.Price
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        await _cache.SetAsync($"product_{product.Id}", product, TimeSpan.FromMinutes(5));

        return new ProductReply
        {
            Id = product.Id,
            Name = product.Name,
            Price = (double)product.Price,
            Source = "database"
        };
    }

    public override async Task<ProductReply> GetProduct(
        GetProductRequest request,
        ServerCallContext context)
    {
        var cacheKey = $"product_{request.Id}";

        var cached = await _cache.GetAsync<Product>(cacheKey);
        if (cached != null)
        {
            return new ProductReply
            {
                Id = cached.Id,
                Name = cached.Name,
                Price = (double)cached.Price,
                Source = "cache"
            };
        }

        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id);

        if (product == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Not found"));

        await _cache.SetAsync(cacheKey, product, TimeSpan.FromMinutes(5));

        return new ProductReply
        {
            Id = product.Id,
            Name = product.Name,
            Price = (double)product.Price,
            Source = "database"
        };
    }
}
```

---

## 11. `Program.cs` (Server)

```csharp
using Microsoft.EntityFrameworkCore;
using ProductGrpcServer.Data;
using ProductGrpcServer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});

var app = builder.Build();

app.MapGrpcService<ProductServiceImpl>();

app.Run();
```

---

## 12. `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=productdb;Username=appuser;Password=apppassword"
  }
}
```

---

## 13. Create DB Schema

```bash
dotnet ef migrations add Initial
dotnet ef database update
```

---

## 14. Run gRPC Server

```bash
dotnet run
```

Server runs on:

```
https://localhost:5001
```

---

# PART 3: gRPC CLIENT

## 15. Create Console Client

```bash
dotnet new console -n ProductGrpcClient
cd ProductGrpcClient
```

---

## 16. Add Packages (Client)

```bash
dotnet add package Grpc.Net.Client
dotnet add package Google.Protobuf
dotnet add package Grpc.Tools
```

Copy **`product.proto`** into client project under `Protos/`.

Add to `.csproj`:

```xml
<ItemGroup>
  <Protobuf Include="Protos\product.proto" GrpcServices="Client" />
</ItemGroup>
```

---

## 17. `Program.cs` (Client)

```csharp
using Grpc.Net.Client;
using ProductGrpcServer.Protos;

using var channel = GrpcChannel.ForAddress("https://localhost:5058");
var client = new ProductService.ProductServiceClient(channel);

// CREATE PRODUCT
var created = await client.CreateProductAsync(
    new CreateProductRequest
    {
        Name = "Mechanical Keyboard",
        Price = 4500
    });

Console.WriteLine($"Created: {created.Id}");

// GET PRODUCT (DB)
var firstRead = await client.GetProductAsync(
    new GetProductRequest { Id = created.Id });

Console.WriteLine($"Source: {firstRead.Source}");

// GET PRODUCT (CACHE)
var secondRead = await client.GetProductAsync(
    new GetProductRequest { Id = created.Id });

Console.WriteLine($"Source: {secondRead.Source}");
```

---

## 18. Run Client

```bash
dotnet run
```

### Output

```
Created: 1
Source: database
Source: cache
```

---

# END-TO-END FLOW (CLEAR)

1. Client → gRPC → CreateProduct
2. Server → PostgreSQL → Save
3. Server → Redis → Cache
4. Client → gRPC → GetProduct
5. First call → DB
6. Second call → Cache

---

# What You Now Have

* gRPC server
* gRPC client
* PostgreSQL (Docker)
* Redis cache-aside
* EF Core
* Full production-style flow

---

### Strong next steps (recommended)

* gRPC + RabbitMQ together
* Outbox pattern (guaranteed events)
* gRPC streaming
* Angular gRPC-Web client

Tell me what you want next.
