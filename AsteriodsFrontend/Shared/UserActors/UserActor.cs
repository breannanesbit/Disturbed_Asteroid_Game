using Akka.Actor;

namespace Actors.UserActors
{
    public class UserActor : ReceiveActor
    {
        public UserState CurrentState { get; set; }
        public User CurrentUser { get; set; } = new User();
        public UserActor()
        {
            Receive<User>(user =>
            {
                //Make http call to raft to save the user info and then set CurrentUser
                CurrentUser.Username = user.Username;
                CurrentUser.Path = Self.Path.ToString();
                Sender.Tell(CurrentUser);
            });

            Receive<ChangeUserState>(user =>
            {
                CurrentState = user.ChangedState;
                Console.WriteLine($"user state is now : {CurrentState}");
            });
        }

        public static Props Props() =>
   Akka.Actor.Props.Create(() => new UserActor());
    }
}
