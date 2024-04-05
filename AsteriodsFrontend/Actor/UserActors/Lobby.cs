using Akka.Actor;

namespace Actors.UserActors
{
    public enum State { Joining, Playing, Over }

    public class LobbyActor : ReceiveActor
    {
        public State CurrentState { get; set; }
        public List<User> Players { get; set; }
        public LobbyActor()
        {
            Players = new List<User>();

            Receive<Lobby>((lobby) =>
            {
                CurrentState = State.Joining;
                Players.Add(new User() { Username = lobby.HeadPlayer });

                Console.WriteLine($"Created a new state");
            });
        }
        public static Props Props() =>
            Akka.Actor.Props.Create(() => new LobbyActor());
    }
}
