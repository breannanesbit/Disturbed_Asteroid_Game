using Actors.UserActors;
using Microsoft.AspNetCore.SignalR;

namespace SignalRAPI
{
    public class ComunicationHub : Hub
    {
        public async Task SendMessage(GameLobby message)
        {
            Console.WriteLine($"in hub : {message.HeadPlayer.hubConnection}");


            foreach(User player in message.Players)
            {
                var client = Clients.Client(player.hubConnection);
                Console.WriteLine($"hub connection: {client}");
                await client.SendAsync("ReceiveMessage", message);

            }
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
