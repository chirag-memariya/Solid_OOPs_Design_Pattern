using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

var builder = WebApplication.CreateBuilder(args);

// 1) Register Mvc + apii services
builder.Services.AddControllers();
builder.Services.AddScoped<IGreetingService, GreetingService>();// sample DI

// register the swagger generator
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//enable swagger middleware in program.cs
if(app.Environment.IsDevelopment()){
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 2) Middleware order(routing before auth, etc)
app.UseHttpsRedirection();
app.UseRouting();

//a placeholder auth middleware(showing metadata use), in real apps useauthentication/useauthorization

app.Use(async (ctx,next)=>{
    //just log matched endpont metadata if present
    var endpoint = ctx.GetEndpoint();
    if(endpoint != null) Console.WriteLine("Endpoint matched: "+endpoint.DisplayName);
    await next();
});

app.UseAuthorization();//must be after UseRouting

// 3) Map Controllers -> create endpoints from controller actions
app.MapControllers();
app.Run();