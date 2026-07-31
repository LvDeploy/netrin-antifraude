using NetrinAF.Api.Configurations;
using NetrinAF.Api.Registers;
using NetrinAF.Application.Middleware.Correlation;
using NetrinAF.Application.Middleware.Idempotency;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationServices(builder.Configuration["AppConfiguration:AppAssemblyName"]!);
//builder.Services.AddInfraServices();
builder.AddHealthCheckConfiguration();
builder.AddSerilogConfiguration();
builder.AddWebApiConfiguration();
builder.AddSwaggerConfiguration();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<IdempotencyKeyMiddleware>();
app.UseWebApplicationConfiguration();
app.UseHealthCheckConfiguration();
app.UseSwaggerConfiguration(app.Environment, app.DescribeApiVersions());
app.Run();

