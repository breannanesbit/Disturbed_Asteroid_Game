using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.Classes;

public class SomeActor :ReceiveActor
{
    public SomeActor()
    {
        Receive<string>(message =>
        {
            Sender.Tell(Self.Path.ToString());
        });
    }
}
