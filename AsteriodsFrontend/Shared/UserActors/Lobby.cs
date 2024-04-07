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
        public GameState CurrentState { get; set; }
        public List<User> Players { get; set; }
        public User HeadPlayer { get; set; }
        public LobbyActor()
        {
            _httpClient = new HttpClient();
            Players = new List<User>();

            Receive<Lobby>((lobby) =>
            {
                CurrentState = GameState.Joining;
                HeadPlayer = new User() { Username = lobby.HeadPlayer };
                Players.Add(HeadPlayer);

                TalkToGateway(lobby);
                Console.WriteLine($"Created a new state");

                Sender.Tell(lobby);
            });

            Receive<ChangeGameState>(state =>
            {
                if (HeadPlayer.Username == state.user)
                {
                    CurrentState = state.lobbyState;
                    Sender.Tell(CurrentState);
                }
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
