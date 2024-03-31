using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.Classes;

public class PrintMyActorRefActor : ReceiveActor
{
    public PrintMyActorRefActor()
    {
        Receive<string>(message => {
            switch (message)
            {
                case "printit":
                    IActorRef secondRef = Context.ActorOf(Props.Empty, "second-actor");
                    Console.WriteLine($"Second: {secondRef}");
                    break;
            }
        });
    }
}