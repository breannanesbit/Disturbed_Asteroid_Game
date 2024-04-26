using Microsoft.AspNetCore.SignalR.Client;

namespace AsteriodWeb
{
    public class SignalRFrontendService
    {

        public HubConnection hubConnection { get; set; }
        public SignalRFrontendService()
        {
            hubConnection = new HubConnectionBuilder()
              .WithUrl("http://signalrapi:8080/ComunicationHub")
              .Build();
        }

        public async Task<bool> IsConnectedAsync()
        {
            try
            {
                if (hubConnection.State == HubConnectionState.Connected)
                {
                    return true;
                }
                else if (hubConnection.State == HubConnectionState.Disconnected ||
                         hubConnection.State == HubConnectionState.Reconnecting)
                {
                    await hubConnection.StartAsync();
                    return hubConnection.State == HubConnectionState.Connected;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions, e.g., connection errors
                Console.WriteLine($"Error checking connection status: {ex.Message}");
                return false;
            }

        }
        public async ValueTask DisposeAsync()
        {
            if (hubConnection != null)
                await hubConnection.DisposeAsync();
        }
    }
}
