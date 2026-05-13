using EfCoreCacheAsidedemo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensibility;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options=>
options.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")
));

//distributed cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration ="localhost:6379";
    options.InstanceName ="EfCoreCache";
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.MapControllers();

app.Run();