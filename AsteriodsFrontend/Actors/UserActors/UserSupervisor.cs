using Actors.UserActors;
using Akka.Actor;

public class UserSupervisor : ReceiveActor
{
    private List<UsersActorInfo> UserActors { get; set; }

    public UserSupervisor()
    {
        UserActors = new List<UsersActorInfo>();

        Receive<string>(username =>
        {
            // Check if a UserActor already exists for the given username
            var existingUser = UserActors.Find(user => user.Username == username);

            if (existingUser == null)
            {
                // If not, create a new UserActor and add it to the list
                var newUserActor = Context.ActorOf(UserActor.Props(), username);
                UserActors.Add(new UsersActorInfo { Username = username, ActorRef = newUserActor });
                newUserActor.Forward(username);

                Console.WriteLine($"New UserActor created for {username}");
            }
            else
            {
                Console.WriteLine($"UserActor already exists for {username}");
            }
        });
    }
    public static Props Props() =>
         Akka.Actor.Props.Create(() => new UserSupervisor());

}

