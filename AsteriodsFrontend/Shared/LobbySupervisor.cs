using Akka.Actor;
using Microsoft.Extensions.Logging;
using Shared.Metrics;
using Shared.SignalRService;

namespace Actors.UserActors
{
    public class LobbySupervisor : ReceiveActor
    {
        private readonly ILogger<LobbySupervisor> logger;

        private List<Lobby> Lobbies { get; set; }
        public IActorRef SignalRActor { get; }

        public LobbySupervisor(ActorSignalRService signalRService, ILogger<LobbySupervisor> logger)
        {
            var props = Props.Create(() => new SignalRActor(signalRService));
            this.SignalRActor = Context.ActorOf(props, "signalRActor");

            this.logger = logger;
            Lobbies = new List<Lobby>();


            Receive<NewLobbyObject>(NewLobby =>
            {
                try
                {
                    var existingUser = Lobbies.Find(u => u.HeadPlayer.Username == NewLobby.username);

                    if (existingUser == null)
                    {
                        var id = Guid.NewGuid();
                        var newLobbyActor = Context.ActorOf(LobbyActor.Props(), id.ToString());

                        // Increase the lobby counter when a new lobby is created
                        DefineMetrics.LobbyCounter.Add(1);

                        var user = new User() { Username = NewLobby.username, hubConnection = NewLobby.hubConnection };

                        var lobby = new Lobby { HeadPlayer = user, ActorRef = newLobbyActor, Id = Guid.NewGuid() };
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

            Receive<GameLobby>(CreadtedLobby =>
            {
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
        //public static Props Props() =>
        //    Akka.Actor.Props.Create(() => new LobbySupervisor());
    }
}