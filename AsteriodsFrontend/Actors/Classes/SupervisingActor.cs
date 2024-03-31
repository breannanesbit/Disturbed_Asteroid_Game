using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.Classes;

public class SupervisingActor : ReceiveActor
{
    public SupervisingActor()
    {
        Receive<string>(message =>
        {
            switch (message)
            {
                case "failChild":
                    child.Tell("fail");
                    break;
            }
        });
    }
    private IActorRef child = Context.ActorOf(Props.Create<SupervisedActor>(), "supervised-actor");
}

public class SupervisedActor : ReceiveActor
{
    public SupervisedActor()
    {
        Receive<string>(message =>
        {
            switch (message)
            {
                case "fail":
                    Console.WriteLine("supervised actor fails now");
                    throw new Exception("I failed!");
            }
        });
    }
    protected override void PreStart() => Console.WriteLine("supervised actor started");
    protected override void PostStop() => Console.WriteLine("supervised actor stopped");
}
