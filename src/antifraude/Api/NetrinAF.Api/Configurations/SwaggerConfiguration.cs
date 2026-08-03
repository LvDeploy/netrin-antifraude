using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using NetrinAF.Api.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NetrinAF.Api.Configurations
{
    internal static class SwaggerConfiguration
    {
        internal static void AddSwaggerConfiguration(this WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            builder.Services.AddSwaggerGen(options =>
            {
                options.OperationFilter<SwaggerDefaultValues>();
            });
        }

        internal static void UseSwaggerConfiguration(this IApplicationBuilder app, IWebHostEnvironment env, IReadOnlyList<ApiVersionDescription> descriptions)
        {
            if (env != null && env.EnvironmentName == "Production")
                return;

            app.UseSwagger(options =>
            {
                options.RouteTemplate = $"{ApiInfo.BaseUrl}/swagger/{{documentName}}/swagger.json";
            });
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = $"{ApiInfo.BaseUrl}/swagger";
                foreach (var description in descriptions.OrderByDescending(p => p.GroupName))
                {
                    string url = $"/{ApiInfo.BaseUrl}/swagger/{description.GroupName}/swagger.json";
                    string name = description.GroupName.ToUpperInvariant();
                    options.SwaggerEndpoint(url, name);
                }
            });
        }
    }
}
