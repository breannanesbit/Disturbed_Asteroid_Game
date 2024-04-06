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

            Receive<CreatedLobby>(CreadtedLobby =>
            {
                SignalRActor.Tell(CreadtedLobby);
            });
        }
        //public static Props Props() =>
        //    Akka.Actor.Props.Create(() => new LobbySupervisor());
    }
}