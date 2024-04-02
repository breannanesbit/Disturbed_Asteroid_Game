using Akka.Actor;

namespace Actors.UserActors
{
    public class LobbySupervisor
    {

        public static Props Props() =>
            Akka.Actor.Props.Create(() => new UserSupervisor());
    }
}