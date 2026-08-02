using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace NetrinAF.Api.Configurations
{
    internal static class TelemetryConfiguration
    {
        private const string ServiceResourceName = "netrin-af-api";
        private const string ServiceResourceVersion = "1.0";

        internal static IOpenTelemetryBuilder AddOpenTelemetryConfiguration(this WebApplicationBuilder builder)
        {
            Uri endpoint = GetOtlpEndpoint(builder.Configuration);

            return builder.Services.AddOpenTelemetry()
                   .ConfigureResource(resource => resource.AddService(
                       ServiceResourceName,
                       serviceVersion: ServiceResourceVersion))
                   .WithTracing(tracing => tracing
                      .AddSource(ServiceResourceName)
                      .SetResourceBuilder(GetResourceBuilder())
                      .AddEntityFrameworkCoreInstrumentation()
                      .AddAspNetCoreInstrumentation()
                      .AddHttpClientInstrumentation()
                      .AddConsoleExporter()
                      .AddOtlpExporter(options => options.Endpoint = endpoint))
                   .WithMetrics(metrics => metrics
                      .SetResourceBuilder(GetResourceBuilder())
                      .AddAspNetCoreInstrumentation()
                      .AddHttpClientInstrumentation()
                      .AddOtlpExporter(options => options.Endpoint = endpoint));
        }

        internal static ILoggingBuilder AddOpenTelemetryLoggingConfiguration(this WebApplicationBuilder builder)
        {
            Uri endpoint = GetOtlpEndpoint(builder.Configuration);

            return builder.Logging.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(GetResourceBuilder());
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.ParseStateValues = true;
                options.AddOtlpExporter(exporter => exporter.Endpoint = endpoint);
                options.AddConsoleExporter();
            });
        }

        private static ResourceBuilder GetResourceBuilder()
        {
            return ResourceBuilder.CreateDefault()
                 .AddService(
                     serviceName: ServiceResourceName,
                     serviceVersion: ServiceResourceVersion);
        }

        private static Uri GetOtlpEndpoint(IConfiguration configuration)
        {
            string value = configuration["OtlpExporter:Endpoint"]
                ?? throw new InvalidOperationException(
                    "OpenTelemetry configuration 'OtlpExporter:Endpoint' was not provided.");

            if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? endpoint))
            {
                throw new InvalidOperationException(
                    "OpenTelemetry configuration 'OtlpExporter:Endpoint' must be an absolute URI.");
            }

            return endpoint;
        }
    }
}
