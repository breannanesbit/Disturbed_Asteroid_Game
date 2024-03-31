using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.Classes;

public class IotApp
{
    public static void Init()
    {
        using (var system = ActorSystem.Create("iot-system"))
        {
            var supervisor = system.ActorOf(IotSupervisor.Props(), "iot-supervisor");
            Console.ReadLine();
        }
    }
}
