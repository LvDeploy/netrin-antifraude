using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace NetrinAF.Api.Configurations
{
    [ExcludeFromCodeCoverage]
    public static class SerilogConfiguration
    {

        public static void AddSerilogConfiguration(this WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();

            IConfigurationBuilder _configuration = new ConfigurationBuilder()
                .AddJsonFile(path: $"appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(path: $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);

            var settings = _configuration.Build();

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .ReadFrom.Configuration(settings)
                .WriteTo.Console()
                .CreateLogger();

            builder.Logging.AddSerilog(Log.Logger, dispose: true);
        }
    }
}
