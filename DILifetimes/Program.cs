using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddRazorPages();

builder.Services.AddTransient<IOperationTransient, Operation>();
builder.Services.AddScoped<IOperationScoped, Operation>();
builder.Services.AddSingleton<IOperationSingleton, Operation>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.MapControllers();


app.Run();

public interface IOperation
{
    string OperationId { get; }
}

public interface IOperationTransient : IOperation { }
public interface IOperationScoped : IOperation { }
public interface IOperationSingleton : IOperation { }


public class Operation : IOperationTransient, IOperationScoped, IOperationSingleton
{
    public Operation()
    {
        var tmp = Guid.NewGuid().ToString();
        OperationId = tmp.Substring(tmp.Length - 4);
        // OperationId = tmp[^4..];
    }

    public string OperationId { get; }
}

