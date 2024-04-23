using Akka.Actor;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared;
using System.Net.Http.Json;

namespace Actors.UserActors
{
    public enum GameState { Joining, Playing, Over }

    public class LobbyActor : ReceiveActor
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LobbyActor> logger;

        public GameLobby CurrentLobby { get; set; } = new GameLobby();
        public LobbyActor(ILogger<LobbyActor> logger)
        {
            _httpClient = new HttpClient();

            Receive<Lobby>((lobby) =>
            {
                Console.WriteLine("made it to the lobby actor");
                logger.LogInformation("creating new lobby in the actor");
                CurrentLobby.CurrentState = GameState.Joining;
                CurrentLobby.HeadPlayer = lobby.HeadPlayer;
                CurrentLobby.Id = lobby.Id;

                var findUser = CurrentLobby.Players.Find(x => x.Username == lobby.HeadPlayer.Username && x.hubConnection == lobby.HeadPlayer.hubConnection);
                if (findUser == null)
                {
                    CurrentLobby.Players.Add(CurrentLobby.HeadPlayer);
                }

                TalkToGateway(lobby);
                Console.WriteLine($"Created a new state");

                Sender.Tell(CurrentLobby);
            });

            Receive<ChangeGameState>(state =>
            {
                if (CurrentLobby.HeadPlayer.Username == state.user)
                {
                    logger.LogInformation($"Changing {state.lobbyId} state");
                    CurrentLobby.CurrentState = state.lobbyState;
                    Sender.Tell(CurrentLobby.CurrentState);
                }
            });

            Receive<AddUserToLobby>(state =>
            {
                // Check if the user is already in the lobby
                if (CurrentLobby.Players.Any(u => u.Username == state.username))
                {
                    Sender.Tell(CurrentLobby.CurrentState); // Reply with the current player count
                }
                else
                {
                    // Add the user to the lobby
                    CurrentLobby.Players.Add(new User { Username = state.username });
                    // Reply with the updated lobby state
                    Sender.Tell(CurrentLobby.CurrentState);
                }
            });

            Receive<DecreaseUserHealth>(userHealth =>
            {
                var user = CurrentLobby.Players.Find(u => u.Username == userHealth.User.Username);
                if (user != null)
                {
                    // Decrease the user's health
                    user.Ship.Health -= userHealth.Damage;

                    // If the user's health is zero or less, remove them from the lobby
                    if (user.Ship.Health <= 0)
                    {
                        CurrentLobby.Players.Remove(user);
                        if (CurrentLobby.Players.Count == 0)
                        {
                            CurrentLobby.CurrentState = GameState.Over;
                        }
                    }
                    Sender.Tell(CurrentLobby);
                }

            });
            this.logger = logger;
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
            Akka.Actor.Props.Create(() => new LobbyActor(new LoggerFactory().CreateLogger<LobbyActor>()));
    }
}
