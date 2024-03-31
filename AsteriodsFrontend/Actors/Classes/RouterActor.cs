using Akka.Actor;
using Akka.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.Classes;
public class RouterActor :ReceiveActor
{
    private readonly IActorRef router;
    public RouterActor()
    {
        router = Context.ActorOf(Props.Empty.WithRouter(new ConsistentHashingPool(5)), "hashing-pool-router");

        Receive<string>(key =>
        {
            router.Tell(new ConsistentHashableEnvelope(message: key, hashKey: key));
        });
    }

}
