using Akka.Actor;
using Akka.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.Classes;

public static partial class MainDeviceGroup
{
    public sealed class RequestTrackDevice
    {
        public RequestTrackDevice(string groupId, string deviceId)
        {
            GroupId = groupId;
            DeviceId = deviceId;
        }
        public string GroupId { get; }
        public string DeviceId { get; }
    }
    public sealed class DeviceRegistered
    {
        public static DeviceRegistered Instance { get; } = new DeviceRegistered();
        private DeviceRegistered() { }
    }

    public class DeviceManager : ReceiveActor
    {
        private readonly Dictionary<string, IActorRef> groupIdToActor = new Dictionary<string, IActorRef>();
        private readonly Dictionary<IActorRef, string> actorToGroupId = new Dictionary<IActorRef, string>();

        protected override void PreStart() => Log.Info("DeviceManager started");
        protected override void PostStop() => Log.Info("DeviceManager stopped");

        private ILoggingAdapter Log { get; } = Context.GetLogger();

        public DeviceManager()
        {
            Receive<RequestTrackDevice>(trackMsg =>
            {
                if (groupIdToActor.TryGetValue(trackMsg.GroupId, out var actorRef))
                {
                    actorRef.Forward(trackMsg);
                }
                else
                {
                    Log.Info($"Creating device group actor for {trackMsg.GroupId}");
                    var groupActor = Context.ActorOf(DeviceGroup.Props(trackMsg.GroupId), $"group-{trackMsg.GroupId}");
                    Context.Watch(groupActor);
                    groupActor.Forward(trackMsg);
                    groupIdToActor.Add(trackMsg.GroupId, groupActor);
                    actorToGroupId.Add(groupActor, trackMsg.GroupId);
                }
            });

            Receive<Terminated>(t =>
            {
                var groupId = actorToGroupId[t.ActorRef];
                Log.Info($"Device group actor for {groupId} has been terminated");
                actorToGroupId.Remove(t.ActorRef);
                groupIdToActor.Remove(groupId);
            });
        }

        public static Props Props() => Akka.Actor.Props.Create<DeviceManager>();
    }
}
