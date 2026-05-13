using Microsoft.EntityFrameworkCore;
using ProductGrpcServer.Models;

namespace ProductGrpcServer.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Product> Products => Set<Product>();
}
