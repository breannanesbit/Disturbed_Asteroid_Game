using Actors.UserActors;
using Microsoft.AspNetCore.SignalR;

namespace SignalRAPI.Hub
{
    public class ComunicationHub : DynamicHub
    {
        public async Task SendMessage(GameLobby message)
        {
            Console.WriteLine("in hub");
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task StartGame(GameState message)
        {
            await Clients.All.SendAsync("GameStarted", message);
        }

        public async Task AllLobbiesSend(List<Lobby> lobbies)
        {
            await Clients.All.SendAsync("GetAllLobbies", lobbies);
        }
    }
}
