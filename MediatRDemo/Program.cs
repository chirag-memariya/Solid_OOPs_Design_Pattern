using System.Reflection;
using MediatR;

var builder = WebApplication.CreateBuilder(args);


// Register MediatR handlers from the current assembly
builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.MapControllers();

app.Run();
