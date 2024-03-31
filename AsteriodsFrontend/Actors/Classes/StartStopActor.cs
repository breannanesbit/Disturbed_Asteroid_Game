using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.Classes;

public class StartStopActor1 : ReceiveActor
{
    public StartStopActor1()
    {
        Receive<object>(message =>
        {
            switch (message)
            {
                case "stop":
                    Context.Stop(Self);
                    break;
            }
        });
    }
    protected override void PreStart()
    {
        Console.WriteLine("first started");
        Context.ActorOf(Props.Create<StartStopActor2>(), "second");
    }

    protected override void PostStop() => Console.WriteLine("first stopped");

}

public class StartStopActor2 : ReceiveActor
{
    public StartStopActor2()
    {
        Receive<object>(message =>
        {
        });
    }
    protected override void PreStart() => Console.WriteLine("second started");
    protected override void PostStop() => Console.WriteLine("second stopped");
}
