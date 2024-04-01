using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.Classes;

public  class Worker : ReceiveActor
{
    public Worker()
    {
        Receive<string>(message => {
            Sender.Tell($"Received message: {message}");
            });
    }
    public static Props Props()
    {
        return Akka.Actor.Props.Create<Worker>();
    }
}
