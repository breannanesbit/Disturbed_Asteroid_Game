using Akka.Actor;
using Akka.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.Classes;

public class DeviceGroup : ReceiveActor
{
    private Dictionary<string, IActorRef> deviceIdToActor = new();
    private Dictionary<IActorRef, string> actorToDeviceId = new();
    private long nextCollectionId = 0L;

    public DeviceGroup(string groupId)
    {
        GroupId = groupId;

        Receive<RequestTrackDevice>(trackMsg =>
        {
            if (trackMsg.Groupid.Equals(GroupId))
            {
                if (deviceIdToActor.TryGetValue(trackMsg.DeviceId, out var actorRef))
                {
                    actorRef.Forward(trackMsg);
                }
                else
                {
                    Log.Info($"Creating device actor for {trackMsg.DeviceId}");
                    var deviceActor = Context.ActorOf(Device.Props(trackMsg.Groupid, trackMsg.DeviceId), $"device-{trackMsg.DeviceId}");
                    deviceIdToActor.Add(trackMsg.DeviceId, deviceActor);
                    deviceActor.Forward(trackMsg);
                }
            }
            else
            {
                Log.Warning($"Ignoring TrackDevice request for {trackMsg.Groupid}. This actor is responsible for {GroupId}.");
            }
        });
        Receive<Terminated>(t =>
        {
            var deviceId = actorToDeviceId[t.ActorRef];
            Log.Info($"Device actor for {deviceId} has been terminated");
            actorToDeviceId.Remove(t.ActorRef);
            deviceIdToActor.Remove(deviceId);
        });
        Receive<RequestDevicelist>(deviceList =>
        {
            Sender.Tell(new ReplyDeviceList(deviceList.RequestId, new HashSet<string>(deviceIdToActor.Keys)));
        });
        Receive<RequestAllTemperatures>(r =>
        {
            Context.ActorOf(DeviceGroupQuery.Props(actorToDeviceId, r.RequestId, Sender, TimeSpan.FromSeconds(3)));
        });
    }
    protected override void PreStart() => Log.Info($"Device group {GroupId} started");
    protected override void PostStop() => Log.Info($"Device group {GroupId} stopped");

    protected ILoggingAdapter Log { get; } = Context.GetLogger();
    protected string GroupId { get; }
    public static Props Props(string groupId) => Akka.Actor.Props.Create(() => new DeviceGroup(groupId));
}

public sealed class RequestDevicelist
{
    public RequestDevicelist(long requestId)
    {
        RequestId = requestId;
    }
    public long RequestId { get; }
}

public sealed class ReplyDeviceList
{
    public ReplyDeviceList(long requestId, ISet<string> ids)
    {
        RequestId = requestId;
        Ids = ids;
    }
    public long RequestId { get; }
    public ISet<string> Ids { get; }
}

public sealed class RequestAllTemperatures
{
    public RequestAllTemperatures(long requestId)
    {
        RequestId = requestId;
    }

    public long RequestId { get; }
}

public sealed class RespondAllTemperatures
{
    public RespondAllTemperatures(long requestId, Dictionary<string, ITemperatureReading> temperatures)
    {
        RequestId = requestId;
        Temperatures = temperatures;
    }

    public long RequestId { get; }
    public Dictionary<string, ITemperatureReading> Temperatures { get; }
}

public interface ITemperatureReading
{
}

public sealed class Temperature : ITemperatureReading
{
    public Temperature(double value)
    {
        Value = value;
    }

    public double Value { get; }
}

public sealed class TemperatureNotAvailable : ITemperatureReading
{
    public static TemperatureNotAvailable Instance { get; } = new();
    private TemperatureNotAvailable() { }
}

public sealed class DeviceNotAvailable : ITemperatureReading
{
    public static DeviceNotAvailable Instance { get; } = new();
    private DeviceNotAvailable() { }
}

public sealed class DeviceTimedOut : ITemperatureReading
{
    public static DeviceTimedOut Instance { get; } = new();
    private DeviceTimedOut() { }
}