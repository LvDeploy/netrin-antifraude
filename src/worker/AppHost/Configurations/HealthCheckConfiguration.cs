using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;

namespace AppHost.Configurations
{
    [ExcludeFromCodeCoverage]
    internal static class HealthCheckConfiguration
    {
        private const string BaseUrl = "netrin-af-worker";
        private const string HEALTHCHECKPATH = "health-check";

        internal static void AddHealthCheckConfiguration(this WebApplicationBuilder builder)
        {
            builder.Services.AddHealthChecks()
                .AddSqlServer(builder.Configuration["ConnectionStrings:SqlServer"]!, name: "SqlServerNetrin");
        }

        internal static void UseHealthCheckConfiguration(this WebApplication app)
        {
            app.UseHealthChecks($"/{BaseUrl}/{HEALTHCHECKPATH}",
                new HealthCheckOptions()
                {
                    ResponseWriter = async (context, report) =>
                    {
                        var result = JsonSerializer.Serialize(
                            new
                            {
                                apiVersion = GetVersion(),
                                dotnet = Environment.Version.ToString(),
                                statusApplication = report.Status.ToString(),
                                healthChecks = report.Entries.Select(e => new
                                {
                                    check = e.Key,
                                    status = Enum.GetName(typeof(HealthStatus), e.Value.Status)
                                })
                            },
                            new JsonSerializerOptions() { WriteIndented = true });

                        context.Response.ContentType = MediaTypeNames.Application.Json;
                        await context.Response.WriteAsync(result);
                    }
                }
            );
        }

        private static string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return $"{fvi.FileMajorPart}.{fvi.FileMinorPart}.{fvi.FileBuildPart}";
        }
    }
}
