using NetrinAF.Api.Configurations;
using NetrinAF.Api.Registers;
using NetrinAF.Application.Middleware.Correlation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationServices();
builder.Services.AddInfraServices();
builder.AddHealthCheckConfiguration();
builder.AddSerilogConfiguration();
builder.AddWebApiConfiguration();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseAuthorization();
app.UseWebApplicationConfiguration();
app.UseHealthCheckConfiguration();

app.Run();

