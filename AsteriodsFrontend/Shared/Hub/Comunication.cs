using Actors.UserActors;
using Microsoft.AspNetCore.SignalR;

namespace SignalRAPI
{
    public class ComunicationHub : Hub
    {
        public async Task SendMessage(GameLobby message)
        {
            Console.WriteLine($"in hub : {message.HeadPlayer.hubConnection}");

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
            var client = Clients.Client(lobbies.hubConnection);

            await client.SendAsync("GetAllLobbies", lobbies);
        }
    }
}
