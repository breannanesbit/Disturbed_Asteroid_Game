using Akka.Actor;
using Newtonsoft.Json;
using Shared;

namespace Actors.UserActors
{
    public enum State { Joining, Playing, Over }

    public class LobbyActor : ReceiveActor
    {
        private readonly HttpClient _httpClient;
        public State CurrentState { get; set; }
        public List<User> Players { get; set; }
        public LobbyActor()
        {
            _httpClient = new HttpClient();
            Players = new List<User>();

            Receive<Lobby>((lobby) =>
            {
                CurrentState = State.Joining;
                Players.Add(new User() { Username = lobby.HeadPlayer });

                TalkToGateway(lobby);
                Console.WriteLine($"Created a new state");

                Sender.Tell(new CreatedLobby { LobbyId = lobby.Id });
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
