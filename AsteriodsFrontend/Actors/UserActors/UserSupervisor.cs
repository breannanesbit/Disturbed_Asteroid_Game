using Actors.UserActors;
using Akka.Actor;

public class UserSupervisor : ReceiveActor
{
    private List<UsersActorInfo> UserActors { get; set; }

    public UserSupervisor()
    {
        UserActors = new List<UsersActorInfo>();

        Receive<User>(user =>
        {
            // Check if a UserActor already exists for the given username
            var existingUser = UserActors.Find(u => u.Username == user.Username);

            if (existingUser == null)
            {
                // If not, create a new UserActor and add it to the list
                var newUserActor = Context.ActorOf(UserActor.Props(), user.Username);
                UserActors.Add(new UsersActorInfo { Username = user.Username, ActorRef = newUserActor });
                newUserActor.Forward(user);

                Console.WriteLine($"New UserActor created for {user.Username}");
            }
            else
            {
                Console.WriteLine($"UserActor already exists for {user.Username}");
            }
        });
    }
    public static Props Props() =>
         Akka.Actor.Props.Create(() => new UserSupervisor());

}

