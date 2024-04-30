using Actors;
using Actors.UserActors;
using Akka.Actor;
using Shared.SignalRService;

public class UserSupervisor : ReceiveActor
{
    private List<UsersActorInfo> UserActors { get; set; }
    public IActorRef SignalRActor { get; }

    private readonly IActorRef newLobbySupervisor;

    public UserSupervisor(ActorSignalRService signalRService, IActorRef newLobbySupervisor)
    {
        var props = Props.Create(() => new SignalRActor(signalRService));
        this.SignalRActor = Context.ActorOf(props, "signalRActor");

        this.newLobbySupervisor = newLobbySupervisor;
        UserActors = new List<UsersActorInfo>();

        Receive<User>(user =>
        {
            Console.WriteLine("made it the user sup to add user");
            // Check if a UserActor already exists for the given username
            var existingUser = UserActors.Find(u => u.Username == user.Username);

            if (existingUser == null)
            {
                // If not, create a new UserActor and add it to the list
                var newUserActor = Context.ActorOf(UserActor.Props(), user.Username);
                UserActors.Add(new UsersActorInfo { Username = user.Username, ActorRef = newUserActor });
                newUserActor.Forward(user);
                //newUserActor.Tell(user, Sender);

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
                //SignalRActor.Tell(NewLobby.username);

                var c = new ChangeUserState() { ChangedState = UserState.Playing };

                existingUser.ActorRef.Forward(c);

            }

        });

        Receive<AddUserToLobby>(AddUserToLobby =>
        {
            Console.WriteLine("in user sup for add user to Lobby");
            var existingUser = UserActors.Find(u => u.Username == AddUserToLobby.username);

            if (existingUser != null)
            {
                var c = new ChangeUserState() { ChangedState = UserState.Playing };

                existingUser.ActorRef.Forward(c);
                Console.WriteLine("in user sup sending add user to the lobby sup");
                newLobbySupervisor.Tell(AddUserToLobby);
            }
            else
            {
                Console.WriteLine("Didn't find a user");
            }
        });
    }
    //public static Props Props() =>
    //     Akka.Actor.Props.Create(() => new UserSupervisor());

}

