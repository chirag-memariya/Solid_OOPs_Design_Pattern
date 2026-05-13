using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

//Add controllers
builder.Services.AddControllers();

//Add redis distributed cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration  = "localhost:6379";
    options.InstanceName = "SampleApp";
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//Enable swagger in development only
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.MapControllers();

app.Run();



