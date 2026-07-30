using Asp.Versioning;
using Asp.Versioning.Builder;
using NetrinAF.Api.Endpoints;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization;

namespace NetrinAF.Api.Configurations
{
    [ExcludeFromCodeCoverage]
    internal static class WebApiConfiguration
    {
        internal static void AddWebApiConfiguration(this WebApplicationBuilder builder)
        {
            builder.AddEndpoints(Assembly.GetExecutingAssembly());
            builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
            builder.Services.AddOutputCache(options =>
            {
                options.AddBasePolicy(builder => builder.NoCache());
            });
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1.0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            }).AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                }).EnableApiVersionBinding();
        }

        internal static void UseWebApplicationConfiguration(this WebApplication app)
        {
            app.UseHttpsRedirection();

            ApiVersionSet apiVersionSet = app.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(1.0))
                .Build();

            RouteGroupBuilder routeGroupBuilder = app
                .MapGroup($"{ApiInfo.BaseUrl}/v{{version:apiVersion}}")
                .WithApiVersionSet(apiVersionSet);

            app.MapEndpoints(routeGroupBuilder);
        }
    }
}
