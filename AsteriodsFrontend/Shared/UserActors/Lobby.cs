using Akka.Actor;
using Newtonsoft.Json;
using Shared;
using System.Net.Http.Json;

namespace Actors.UserActors
{
    public enum GameState { Joining, Playing, Over }

    public class LobbyActor : ReceiveActor
    {
        private readonly HttpClient _httpClient;
        public GameLobby CurrentLobby { get; set; } = new GameLobby();
        public LobbyActor()
        {
            _httpClient = new HttpClient();

            Receive<Lobby>((lobby) =>
            {
                CurrentLobby.CurrentState = GameState.Joining;
                CurrentLobby.HeadPlayer = new User() { Username = lobby.HeadPlayer };
                CurrentLobby.Players.Add(CurrentLobby.HeadPlayer);

                TalkToGateway(lobby);
                Console.WriteLine($"Created a new state");

                Sender.Tell(CurrentLobby);
            });

            Receive<ChangeGameState>(state =>
            {
                if (CurrentLobby.HeadPlayer.Username == state.user)
                {
                    CurrentLobby.CurrentState = state.lobbyState;
                    Sender.Tell(CurrentLobby.CurrentState);
                }
            });

            Receive<AddUserToLobby>(state =>
            {
                CurrentLobby.Players.Add(new User() { Username = state.username });
                Sender.Tell(CurrentLobby.Players.Count);
            });
        }



        public void TalkToGateway(Lobby lobby)
        {
            var serializedLobby = JsonConvert.SerializeObject(lobby);


            var kp = new KeyValue
            {
                key = lobby.Id.ToString(),
                value = serializedLobby
            };

            _httpClient.PostAsJsonAsync($"http://asteriodsapi:2010/Gateway/newValue", kp);
        }

        public static Props Props() =>
            Akka.Actor.Props.Create(() => new LobbyActor());
    }
}
