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
                // Check if the user is already in the lobby
                if (CurrentLobby.Players.Any(u => u.Username == state.username))
                {
                    Sender.Tell(CurrentLobby.Players.Count); // Reply with the current player count
                }
                else
                {
                    // Add the user to the lobby
                    CurrentLobby.Players.Add(new User { Username = state.username });
                    // Reply with the updated lobby state
                    Sender.Tell(CurrentLobby.Players.Count);
                }
            });

        }



        public void TalkToGateway(Lobby lobby)
        {
            try
            {
                var serializedLobby = JsonConvert.SerializeObject(lobby);


                var kp = new KeyValue
                {
                    key = lobby.Id.ToString(),
                    value = serializedLobby
                };

                _httpClient.PostAsJsonAsync($"http://asteriodsapi:2010/Gateway/newValue", kp);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static Props Props() =>
            Akka.Actor.Props.Create(() => new LobbyActor());
    }
}
