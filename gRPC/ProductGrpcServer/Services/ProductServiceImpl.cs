using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
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
