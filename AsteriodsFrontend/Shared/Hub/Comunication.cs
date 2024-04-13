using Actors.UserActors;
using Microsoft.AspNetCore.SignalR;

namespace SignalRAPI.Hub
{
    public class ComunicationHub : DynamicHub
    {
        public async Task SendMessage(GameLobby message)
        {
            Console.WriteLine("in hub");

            var client = Clients.Client(message.HeadPlayer.hubConnection);
            Console.WriteLine($"hub connection: {client}");
            await client.SendAsync("ReceiveMessage", message);
        }

        public async Task StartGame(GameState message)
        {
            await Clients.All.SendAsync("GameStarted", message);
        }

        public async Task AllLobbiesSend(AllLobbies lobbies)
        {
            Console.WriteLine("hub all lobbies");
            await Clients.All.SendAsync("GetAllLobbies", lobbies);
        }
    }
}
