using System.Reflection;
using MediatR;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(configuration=>
    configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
);

//register repository + unitofwork 
builder.Services.AddSingleton<IMessageRepository,InMemoryMessageRepository>();
builder.Services.AddSingleton<IUnitOfWork,InMemoryUnitOfWork>();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

//configure the http request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();