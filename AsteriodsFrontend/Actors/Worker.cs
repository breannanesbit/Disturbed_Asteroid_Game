using Akka.Actor;
using Akka.Routing;


namespace Actors;

public class RouterActor : ReceiveActor
{
    public RouterActor()
    {
        // Create a consistent hashing pool router with 5 actors
        var pool = Context.ActorOf(Props.Create<Worker>());

        Receive<string>(message =>
        {
            // Send message to pool router
            pool.Forward(message);
        });
    }
}


public class Worker : ReceiveActor
{
    public Worker()
    {
        Receive<string>((message) =>
        {
            Sender.Tell(Self.Path.ToString());

        });
        // Define message handling logic using Receive<T> methods
        Receive<(string, IActorRef)>((tuple) =>
        {
            var (message, sender) = tuple;
            // Respond directly to the original sender
            sender.Tell(Self.Path.ToString());
        });
    }


    public static Props Props() =>
    Akka.Actor.Props.Create(() => new Worker()).WithRouter(new ConsistentHashingPool(5));
}

public class KeyMessage
{
    public string Key { get; }
    public IActorRef Sender { get; }

    public KeyMessage(string key, IActorRef sender)
    {
        Key = key;
        Sender = sender;
    }
}

public class UserInputActor : ReceiveActor
{
    private IActorRef router;

    public UserInputActor(IActorRef router)
    {
        this.router = router;

        Receive<string>(key =>
        {
            // Send message to router with the user's input key
            router.Tell(new KeyMessage(key, Sender));
        });
    }
}


