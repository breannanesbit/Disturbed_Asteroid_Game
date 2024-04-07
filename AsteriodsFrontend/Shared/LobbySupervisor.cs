using Akka.Actor;

namespace Actors.UserActors
{
    public class LobbySupervisor : ReceiveActor
    {
        private List<Lobby> Lobbies { get; set; }
        public IActorRef SignalRActor { get; }

        public LobbySupervisor(IActorRef SignalRActor)
        {
            this.SignalRActor = SignalRActor;
            Lobbies = new List<Lobby>();


            Receive<NewLobbyObject>(NewLobby =>
            {
                var existingUser = Lobbies.Find(u => u.HeadPlayer == NewLobby.username);

                if (existingUser == null)
                {
                    var newLobbyActor = Context.ActorOf(LobbyActor.Props());
                    var lobby = new Lobby { HeadPlayer = NewLobby.username, ActorRef = newLobbyActor, Id = Guid.NewGuid() };
                    Lobbies.Add(lobby);
                    newLobbyActor.Tell(lobby);

                    Console.WriteLine("made it to the supervisor");
                }
            });

            Receive<GameLobby>(CreadtedLobby =>
            {
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
                    existingUser.ActorRef.Forward(state);
                }
            });
        }
        //public static Props Props() =>
        //    Akka.Actor.Props.Create(() => new LobbySupervisor());
    }
}