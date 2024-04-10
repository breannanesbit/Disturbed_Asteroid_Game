using Actors.UserActors;
using Microsoft.AspNetCore.SignalR;

namespace SignalRAPI.Hub
{
    public class ComunicationHub : DynamicHub
    {
        public async Task SendMessage(Lobby message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task StartGame(GameState message)
        {
            await Clients.All.SendAsync("GameStarted", message);
        }
    }
}
