using Actors.UserActors;
using Microsoft.AspNetCore.SignalR.Client;

namespace Shared.SignalRService;
public class ActorSignalRService
{
    public HubConnection hubConnection { get; set; }
    public ActorSignalRService()
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

    public async Task SendGameLobby(GameLobby gameLobby)
    {
        if (await IsConnectedAsync())
        {
            hubConnection.SendAsync("SendMessage", gameLobby);
        }
    }

    public async Task SendGameState(GameState state)
    {
        if (await IsConnectedAsync())
        {
            hubConnection.SendAsync("StartGame", state);
        }
    }

    public async Task SendAllLobbies(AllLobbies lobbies)
    {
        if (await IsConnectedAsync())
        {
            Console.WriteLine("In signalR service for getting all lobbies");
            await hubConnection.SendAsync("AllLobbiesSend", lobbies);
        }
    }
}
