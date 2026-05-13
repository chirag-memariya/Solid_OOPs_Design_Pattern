using System.Diagnostics;
using WebApi.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

if (Debugger.IsAttached)
{
    // Remove default providers, use Serilog
    builder.Logging.ClearProviders();
    builder.Logging.AddDebug();
}
else
{
    // Configure Serilog for file logging
    Log.Logger = new LoggerConfiguration()
        .WriteTo.File("Logs/webapi-log.txt", rollingInterval: RollingInterval.Day)
        .WriteTo.Console()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();
    builder.Host.UseSerilog();
}


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend",
            policy =>
            {
                policy.WithOrigins("https://192.168.27.79:4201")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials(); // Allow credentials if needed
            }
        );
    }
);

builder.Services.AddSingleton<ConfigService>();
builder.Services.AddSingleton<LoginService>();
builder.Services.AddSingleton<RSService>();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.MapHub<BridgeUtilityHub>("/bridgeUtility");

app.Run();

if (!Debugger.IsAttached)
{// Ensure to flush and close log on shutdown
    Log.CloseAndFlush();
}
