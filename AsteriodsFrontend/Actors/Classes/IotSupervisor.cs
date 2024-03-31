using Akka.Actor;
using Akka.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.Classes;

public class IotSupervisor : ReceiveActor
{
    public IotSupervisor() { Receive<object>(message => { }); }
    public ILoggingAdapter Log { get; } = Context.GetLogger();
    protected override void PreStart() => Log.Info("Iot Application started");
    protected override void PostStop() => Log.Info("Iot Application stopped");

    public static Props Props() => Akka.Actor.Props.Create<IotSupervisor>();
}
