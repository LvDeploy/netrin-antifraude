using AppHost.Configurations;
using AppHost.Registers;

var builder = WebApplication.CreateBuilder(args);
builder.AddRabbitMqConfiguration();
builder.AddEFContextConfiguration();
builder.AddHealthCheckConfiguration();
builder.AddSerilogConfiguration();
// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseApplicationServices();
app.Run();

