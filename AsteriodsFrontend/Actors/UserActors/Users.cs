using Akka.Actor;

namespace Actors.UserActors
{
    public class UsersActorInfo
    {
        public string Username { get; set; }
        public IActorRef ActorRef { get; set; }
    }

    public class User
    {
        public string Username { get; set; }
        public string Path { get; set; }
    }

}