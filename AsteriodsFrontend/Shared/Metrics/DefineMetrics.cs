using System.Diagnostics.Metrics;

namespace Shared.Metrics
{
    public class DefineMetrics
    {
        public static readonly string lobbyMeterName = "AsteriodsMeterics";
        public static Meter lobbyMeter = new Meter(lobbyMeterName);
        // Define the lobby counter metric
        public static readonly UpDownCounter<long> LobbyCounter = lobbyMeter.CreateUpDownCounter<long>(
            "lobby_created_destroyed");
    }
}
