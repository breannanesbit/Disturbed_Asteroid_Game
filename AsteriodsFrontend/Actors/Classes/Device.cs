using Akka.Actor;
using Akka.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.Classes;

public class Device : ReceiveActor
{
    public Device(string groupId, string deviceId)
    {
        GroupId = groupId;
        DeviceId = deviceId;
        Receive<RequestTrackDevice>(req =>
        {
            if (req.Groupid.Equals(GroupId) && req.DeviceId.Equals(DeviceId))
            {
                Sender.Tell(DeviceRegistered.Instance);
            }
            else
            {
                Log.Warning($"Ignoring TrackDevice request for {req.Groupid}-{req.DeviceId}. This actor is responsible for {GroupId}-{DeviceId}.");
            }
        });
        Receive<RecordTemperature>(rec =>
        {
            Log.Info($"Recorded temperature reading {rec.Value} with {rec.RequestId}");
            _lastTemperaturReading = rec.Value;
            Sender.Tell(new TemperatureRecorded(rec.RequestId));
        });
        Receive<ReadTemperature>(read =>
        {
            Sender.Tell(new RespondTemperature(read.RequestId, _lastTemperaturReading));
        });

    }
    private double? _lastTemperaturReading = null;
    protected override void PreStart() => Log.Info($"Device actor {GroupId}-{DeviceId}started");
    protected override void PostStop() => Log.Info($"Device actor {GroupId}-{DeviceId} stopped");

    protected ILoggingAdapter Log { get; } = Context.GetLogger();
    protected string GroupId { get; }
    protected string DeviceId { get; }

    public static Props Props(string groupId, string deviceId) =>
        Akka.Actor.Props.Create(() => new Device(groupId, deviceId));
}
