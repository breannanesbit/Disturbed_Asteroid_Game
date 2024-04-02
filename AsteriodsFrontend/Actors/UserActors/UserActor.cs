using Akka.Actor;

namespace Actors.UserActors
{
    public class UserActor : ReceiveActor
    {
        public User CurrentUser { get; set; } = new User();
        public UserActor()
        {
            Receive<string>(username =>
            {
                //Make http call to raft to save the user info and then set CurrentUser
                CurrentUser.Username = username;
                CurrentUser.Path = Self.Path.ToString();
                Sender.Tell(CurrentUser);
            });
        }

        public static Props Props() =>
   Akka.Actor.Props.Create(() => new UserActor());
    }
}
