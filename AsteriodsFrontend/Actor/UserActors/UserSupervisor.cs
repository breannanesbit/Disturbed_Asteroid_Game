using Actors.UserActors;
using Akka.Actor;

public class UserSupervisor : ReceiveActor
{
    private List<UsersActorInfo> UserActors { get; set; }
    public IActorRef SignalRActor { get; }

    private readonly IActorRef newLobbySupervisor;

    public UserSupervisor(IActorRef SignalRActor, IActorRef newLobbySupervisor)
    {
        this.SignalRActor = SignalRActor;
        this.newLobbySupervisor = newLobbySupervisor;
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

        Receive<NewLobbyObject>(NewLobby =>
        {
            var existingUser = UserActors.Find(u => u.Username == NewLobby.username);

            if (existingUser != null)
            {
                newLobbySupervisor.Tell(NewLobby);
                SignalRActor.Tell(NewLobby.username);

                var c = new ChangeUserState() { ChangedState = UserState.Playing };

                existingUser.ActorRef.Forward(c);

            }

        });
    }
    //public static Props Props() =>
    //     Akka.Actor.Props.Create(() => new UserSupervisor());

}

