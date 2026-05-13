using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Data;
public class ApplicatinoDbContext(DbContextOptions<ApplicatinoDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Product> Products{get;set;}
}