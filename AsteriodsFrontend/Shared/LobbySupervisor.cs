using Akka.Actor;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared;
using Shared.Metrics;
using Shared.SignalRService;
using System.Net.Http.Json;

namespace Actors.UserActors
{
    public class LobbySupervisor : ReceiveActor
    {
        private readonly ILogger<LobbySupervisor> logger;

        private List<Lobby> Lobbies { get; set; }
        public IActorRef SignalRActor { get; }
        private readonly HttpClient _httpClient;


        public LobbySupervisor(ActorSignalRService signalRService, ILogger<LobbySupervisor> logger)
        {
            var props = Props.Create(() => new SignalRActor(signalRService));
            this.SignalRActor = Context.ActorOf(props, "signalRActor");

            this.logger = logger;
            Lobbies = new List<Lobby>();
            _httpClient = new HttpClient();


            Receive<Terminated>(t =>
            {

                var lobby = Lobbies.Find(l => l.ActorRef == t.ActorRef);
                if (lobby != null)
                {
                    var newLobbyActor = Context.ActorOf(LobbyActor.Props(), lobby.Id.ToString());
                    Context.Watch(newLobbyActor);

                    var newlobby = new Lobby { HeadPlayer = lobby.HeadPlayer, ActorRef = newLobbyActor, Id = lobby.Id };
                    Lobbies.Add(lobby);
                }
                else
                {
                    logger.LogError("Didn't find lobby to stand back up");
                }
            });


            Receive<NewLobbyObject>(NewLobby =>
            {
                try
                {
                    var existingUser = Lobbies.Find(u => u.HeadPlayer.Username == NewLobby.username);

                    if (existingUser == null)
                    {
                        var id = Guid.NewGuid();
                        var newLobbyActor = Context.ActorOf(LobbyActor.Props(), id.ToString());
                        Context.Watch(newLobbyActor);

                        // Increase the lobby counter when a new lobby is created
                        DefineMetrics.LobbyCounter.Add(1);

                        var user = new User() { Username = NewLobby.username, hubConnection = NewLobby.hubConnection };

                        var lobby = new Lobby { HeadPlayer = user, ActorRef = newLobbyActor, Id = id };
                        Lobbies.Add(lobby);

                        newLobbyActor.Forward(lobby);
                        newLobbyActor.Tell(lobby);

                        Console.WriteLine("made it to the supervisor");
                        logger.LogInformation("creating a new lobby from the supervisor");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogInformation($"{ex.Message}");
                    Console.WriteLine(ex.ToString());
                }
            });
            Receive<MoveEvent>((MoveEvent) =>
            {
                var existingUser = Lobbies.Find(g => g.Id == MoveEvent.lobbyId);
                if (existingUser != null)
                {
                    existingUser.ActorRef.Forward(MoveEvent);
                }
            });
            Receive<Lazer>((lazer) =>
            {
                var existingUser = Lobbies.Find(g => g.Id == lazer.lobbyId);
                if (existingUser != null)
                {
                    existingUser.ActorRef.Forward(lazer);
                }
            });
            Receive<GameLobby>(CreadtedLobby =>
            {
                TalkToGateway(CreadtedLobby);
                logger.LogInformation($"made it to call to signalR actor {SignalRActor.Path}");
                Console.WriteLine($"made it to call to signalR actor {SignalRActor.Path}");
                SignalRActor.Tell(CreadtedLobby);
            });

            Receive<ChangeGameState>(state =>
            {
                var existingUser = Lobbies.Find(g => g.Id == state.lobbyId);
                if (existingUser != null)
                {
                    existingUser.ActorRef.Tell(state);
                }
            });

            Receive<GameState>(state =>
            {
                SignalRActor.Tell(state);
            });


            Receive<AddUserToLobby>(state =>
            {
                var existingUser = Lobbies.Find(g => g.Id == state.lobbyId);
                if (existingUser != null)
                {
                    logger.LogInformation("adding user to a lobby");
                    existingUser.ActorRef.Forward(state);
                }
            });

            Receive<AllLobbies>(lobbies =>
            {
                logger.LogInformation($"In lobby sup getting all lobbies");
                var all = new AllLobbies()
                {
                    Lobbies = Lobbies,
                };
                SignalRActor.Tell(all);
                Sender.Tell(Lobbies);
            });

            Receive<DecreaseUserHealth>(duh =>
            {
                var existingUser = Lobbies.Find(g => g.Id == duh.LobbyId);
                if (existingUser != null)
                {
                    existingUser.ActorRef.Forward(duh);
                    //existingUser.ActorRef.Tell(duh);
                }
            });
        }

        public void TalkToGateway(GameLobby lobby)
        {
            try
            {
                Console.WriteLine("in talk to gateway");
                var serializedLobby = JsonConvert.SerializeObject(lobby);


                var kp = new KeyValue
                {
                    key = lobby.Id.ToString(),
                    value = serializedLobby
                };

                _httpClient.PostAsJsonAsync($"http://asteriodsapi:8080/Gateway/newValue", kp);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error from talktogateway method: {ex.Message}");
            }
        }
        //public static Props Props() =>
        //    Akka.Actor.Props.Create(() => new LobbySupervisor());
    }
}