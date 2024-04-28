using OpenTelemetry;
using OpenTelemetry.Metrics;
using Shared.Metrics;

namespace AsteriodWeb.LogConfig
{
    public static class MetricsConfiguration
    {
        public static OpenTelemetryBuilder ConfigureMetrics(this OpenTelemetryBuilder builder)
        {
            return builder.WithMetrics(metrics =>
            {
                var lobbyMeter = DefineMetrics.lobbyMeter;

                metrics
                    .AddMeter(lobbyMeter.Name)
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter();

                // Define the lobby counter metric
                var lobbyCounter = DefineMetrics.LobbyCounter;

                // Register the lobby counter for use
                builder.Services.AddSingleton(lobbyCounter);
            });
        }
    }
}
