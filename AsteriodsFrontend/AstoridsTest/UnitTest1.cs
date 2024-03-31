using Actors.Classes;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using Akka.Util.Internal;
using FluentAssertions;

namespace AstoridsTest;

public class UnitTest1 : TestKit
{
    [Fact]
    public void Device_actor_must_reply_with_empty_reading_if_no_temperature_is_known()
    {
        var Sys = ActorSystem.Create("MyActorSystem");
        var probe = CreateTestProbe();
        var deviceActor = Sys.ActorOf(Device.Props("group", "device"));

        deviceActor.Tell(new ReadTemperature(requestId: 42), probe.Ref);
        var response = probe.ExpectMsg<RespondTemperature>();
        response.RequestId.Should().Be(42);
        response.Value.Should().Be(null);
    }

    [Fact]
    public void Device_actor_must_reply_with_latest_temperature_reading()
    {
        var Sys = ActorSystem.Create("MyActorSystem");
        var probe = CreateTestProbe();
        var deviceActor = Sys.ActorOf(Device.Props("group", "device"));

        deviceActor.Tell(new RecordTemperature(requestId: 1, value: 24.0), probe.Ref);
        probe.ExpectMsg<TemperatureRecorded>(s => s.RequestId == 1);

        deviceActor.Tell(new ReadTemperature(requestId: 2), probe.Ref);
        var response1 = probe.ExpectMsg<RespondTemperature>();
        response1.RequestId.Should().Be(2);
        response1.Value.Should().Be(24.0);

        deviceActor.Tell(new RecordTemperature(requestId: 3, value: 55.0), probe.Ref);
        probe.ExpectMsg<TemperatureRecorded>(s => s.RequestId == 3);

        deviceActor.Tell(new ReadTemperature(requestId: 4), probe.Ref);
        var response2 = probe.ExpectMsg<RespondTemperature>();
        response2.RequestId.Should().Be(4);
        response2.Value.Should().Be(55.0);
    }
    [Fact]
    public void Device_actor_must_reply_to_registration_requests()
    {
        var probe = CreateTestProbe();
        var deviceActor = Sys.ActorOf(Device.Props("group", "device"));

        deviceActor.Tell(new RequestTrackDevice("group", "device"), probe.Ref);
        probe.ExpectMsg<DeviceRegistered>();
        probe.LastSender.Should().Be(deviceActor);
    }

    [Fact]
    public void Device_actor_must_ignore_wrong_registration_requests()
    {
        var probe = CreateTestProbe();
        var deviceActor = Sys.ActorOf(Device.Props("group", "device"));

        deviceActor.Tell(new RequestTrackDevice("wrongGroup", "device"), probe.Ref);
        probe.ExpectNoMsg(TimeSpan.FromMilliseconds(500));

        deviceActor.Tell(new RequestTrackDevice("group", "Wrongdevice"), probe.Ref);
        probe.ExpectNoMsg(TimeSpan.FromMilliseconds(500));
    }
    [Fact]
    public void DeviceGroup_actor_must_be_able_to_register_a_device_actor()
    {
        var probe = CreateTestProbe();
        var groupActor = Sys.ActorOf(DeviceGroup.Props("group"));

        groupActor.Tell(new RequestTrackDevice("group", "device1"), probe.Ref);
        probe.ExpectMsg<DeviceRegistered>();
        var deviceActor1 = probe.LastSender;

        groupActor.Tell(new RequestTrackDevice("group", "device2"), probe.Ref);
        probe.ExpectMsg<DeviceRegistered>();
        var deviceActor2 = probe.LastSender;
        deviceActor1.Should().NotBe(deviceActor2);

        // Check that the device actors are working
        deviceActor1.Tell(new RecordTemperature(requestId: 0, value: 1.0), probe.Ref);
        probe.ExpectMsg<TemperatureRecorded>(s => s.RequestId == 0);
        deviceActor2.Tell(new RecordTemperature(requestId: 1, value: 2.0), probe.Ref);
        probe.ExpectMsg<TemperatureRecorded>(s => s.RequestId == 1);
    }

    [Fact]
    public void DeviceGroup_actor_must_ignore_requests_for_wrong_groupId()
    {
        var probe = CreateTestProbe();
        var groupActor = Sys.ActorOf(DeviceGroup.Props("group"));

        groupActor.Tell(new RequestTrackDevice("wrongGroup", "device1"), probe.Ref);
        probe.ExpectNoMsg(TimeSpan.FromMilliseconds(500));
    }
    [Fact]
    public void DeviceGroup_actor_must_return_same_actor_for_same_deviceId()
    {
        var probe = CreateTestProbe();
        var groupActor = Sys.ActorOf(DeviceGroup.Props("group"));

        groupActor.Tell(new RequestTrackDevice("group", "device1"), probe.Ref);
        probe.ExpectMsg<DeviceRegistered>();
        var deviceActor1 = probe.LastSender;

        groupActor.Tell(new RequestTrackDevice("group", "device1"), probe.Ref);
        probe.ExpectMsg<DeviceRegistered>();
        var deviceActor2 = probe.LastSender;

        deviceActor1.Should().Be(deviceActor2);
    }
    [Fact]
    public void DeviceGroup_actor_must_be_able_to_list_active_devices()
    {
        var probe = CreateTestProbe();
        var groupActor = Sys.ActorOf(DeviceGroup.Props("group"));

        groupActor.Tell(new RequestTrackDevice("group", "device1"), probe.Ref);
        probe.ExpectMsg<DeviceRegistered>();

        groupActor.Tell(new RequestTrackDevice("group", "device2"), probe.Ref);
        probe.ExpectMsg<DeviceRegistered>();

        groupActor.Tell(new RequestDevicelist(requestId: 0), probe.Ref);
        probe.ExpectMsg<ReplyDeviceList>(s => s.RequestId == 0
            && s.Ids.Contains("device1")
            && s.Ids.Contains("device2"));
    }

    [Fact]
    public void DeviceGroup_actor_must_be_able_to_list_active_devices_after_one_shuts_down()
    {
        var probe = CreateTestProbe();
        var groupActor = Sys.ActorOf(DeviceGroup.Props("group"));

        groupActor.Tell(new RequestTrackDevice("group", "device1"), probe.Ref);
        probe.ExpectMsg<DeviceRegistered>();
        var toShutDown = probe.LastSender;

        groupActor.Tell(new RequestTrackDevice("group", "device2"), probe.Ref);
        probe.ExpectMsg<DeviceRegistered>();

        groupActor.Tell(new RequestDevicelist(requestId: 0), probe.Ref);
        probe.ExpectMsg<ReplyDeviceList>(s => s.RequestId == 0
                                              && s.Ids.Contains("device1")
                                              && s.Ids.Contains("device2"));

        probe.Watch(toShutDown);
        toShutDown.Tell(PoisonPill.Instance);
        probe.ExpectTerminated(toShutDown);

        // using awaitAssert to retry because it might take longer for the groupActor
        // to see the Terminated, that order is undefined
        probe.AwaitAssert(() =>
        {
            groupActor.Tell(new RequestDevicelist(requestId: 1), probe.Ref);
            probe.ExpectMsg<ReplyDeviceList>(s => s.RequestId == 1 && s.Ids.Contains("device2") && !s.Ids.Contains("device1"));
        });
    }
    [Fact]
    public void DeviceGroupQuery_must_return_temperature_value_for_working_devices()
    {
        var requester = CreateTestProbe();

        var device1 = CreateTestProbe();
        var device2 = CreateTestProbe();

        var queryActor = Sys.ActorOf(DeviceGroupQuery.Props(
            actorToDeviceId: new Dictionary<IActorRef, string> { [device1.Ref] = "device1", [device2.Ref] = "device2" },
            requestId: 1,
            requester: requester.Ref,
            timeout: TimeSpan.FromSeconds(3)
        ));

        device1.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);
        device2.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);

        queryActor.Tell(new RespondTemperature(requestId: 0, value: 1.0), device1.Ref);
        queryActor.Tell(new RespondTemperature(requestId: 0, value: 2.0), device2.Ref);

        requester.ExpectMsg<RespondAllTemperatures>(msg =>
            msg.Temperatures["device1"].AsInstanceOf<Temperature>().Value == 1.0 &&
            msg.Temperatures["device2"].AsInstanceOf<Temperature>().Value == 2.0 &&
            msg.RequestId == 1);
    }
    [Fact]
    public void DeviceGroupQuery_must_return_TemperatureNotAvailable_for_devices_with_no_readings()
    {
        var requester = CreateTestProbe();

        var device1 = CreateTestProbe();
        var device2 = CreateTestProbe();

        var queryActor = Sys.ActorOf(DeviceGroupQuery.Props(
            actorToDeviceId: new Dictionary<IActorRef, string> { [device1.Ref] = "device1", [device2.Ref] = "device2" },
            requestId: 1,
            requester: requester.Ref,
            timeout: TimeSpan.FromSeconds(3)
        ));

        device1.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);
        device2.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);

        queryActor.Tell(new RespondTemperature(requestId: 0, value: null), device1.Ref);
        queryActor.Tell(new RespondTemperature(requestId: 0, value: 2.0), device2.Ref);

        requester.ExpectMsg<RespondAllTemperatures>(msg =>
            msg.Temperatures["device1"] is TemperatureNotAvailable &&
            msg.Temperatures["device2"].AsInstanceOf<Temperature>().Value == 2.0 &&
            msg.RequestId == 1);
    }
    [Fact]
    public void DeviceGroupQuery_must_return_return_DeviceNotAvailable_if_device_stops_before_answering()
    {
        var requester = CreateTestProbe();

        var device1 = CreateTestProbe();
        var device2 = CreateTestProbe();

        var queryActor = Sys.ActorOf(DeviceGroupQuery.Props(
            actorToDeviceId: new Dictionary<IActorRef, string> { [device1.Ref] = "device1", [device2.Ref] = "device2" },
            requestId: 1,
            requester: requester.Ref,
            timeout: TimeSpan.FromSeconds(3)
        ));

        device1.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);
        device2.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);

        queryActor.Tell(new RespondTemperature(requestId: 0, value: 1.0), device1.Ref);
        device2.Tell(PoisonPill.Instance);

        requester.ExpectMsg<RespondAllTemperatures>(msg =>
            msg.Temperatures["device1"].AsInstanceOf<Temperature>().Value == 1.0 &&
            msg.Temperatures["device2"] is DeviceNotAvailable &&
            msg.RequestId == 1);
    }
    [Fact]
    public void DeviceGroupQuery_must_return_temperature_reading_even_if_device_stops_after_answering()
    {
        var requester = CreateTestProbe();

        var device1 = CreateTestProbe();
        var device2 = CreateTestProbe();

        var queryActor = Sys.ActorOf(DeviceGroupQuery.Props(
            actorToDeviceId: new Dictionary<IActorRef, string> { [device1.Ref] = "device1", [device2.Ref] = "device2" },
            requestId: 1,
            requester: requester.Ref,
            timeout: TimeSpan.FromSeconds(3)
        ));

        device1.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);
        device2.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);

        queryActor.Tell(new RespondTemperature(requestId: 0, value: 1.0), device1.Ref);
        queryActor.Tell(new RespondTemperature(requestId: 0, value: 2.0), device2.Ref);
        device2.Tell(PoisonPill.Instance);

        requester.ExpectMsg<RespondAllTemperatures>(msg =>
            msg.Temperatures["device1"].AsInstanceOf<Temperature>().Value == 1.0 &&
            msg.Temperatures["device2"].AsInstanceOf<Temperature>().Value == 2.0 &&
            msg.RequestId == 1);
    }
    [Fact]
    public void DeviceGroupQuery_must_return_DeviceTimedOut_if_device_does_not_answer_in_time()
    {
        var requester = CreateTestProbe();

        var device1 = CreateTestProbe();
        var device2 = CreateTestProbe();

        var queryActor = Sys.ActorOf(DeviceGroupQuery.Props(
            actorToDeviceId: new Dictionary<IActorRef, string> { [device1.Ref] = "device1", [device2.Ref] = "device2" },
            requestId: 1,
            requester: requester.Ref,
            timeout: TimeSpan.FromSeconds(1)
        ));

        device1.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);
        device2.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);

        queryActor.Tell(new RespondTemperature(requestId: 0, value: 1.0), device1.Ref);

        requester.ExpectMsg<RespondAllTemperatures>(msg =>
            msg.Temperatures["device1"].AsInstanceOf<Temperature>().Value == 1.0 &&
            msg.Temperatures["device2"] is DeviceTimedOut &&
            msg.RequestId == 1);
    }
    [Fact]
    public void DeviceGroup_actor_must_be_able_to_collect_temperatures_from_all_active_devices()
    {
        var probe = CreateTestProbe();
        var groupActor = Sys.ActorOf(DeviceGroup.Props("group"));

        groupActor.Tell(new RequestTrackDevice("group", "device1"), probe.Ref);
        probe.ExpectMsg<DeviceRegistered>();
        var deviceActor1 = probe.LastSender;

        groupActor.Tell(new RequestTrackDevice("group", "device2"), probe.Ref);
        probe.ExpectMsg<DeviceRegistered>();
        var deviceActor2 = probe.LastSender;

        groupActor.Tell(new RequestTrackDevice("group", "device3"), probe.Ref);
        probe.ExpectMsg<DeviceRegistered>();
        var deviceActor3 = probe.LastSender;

        // Check that the device actors are working
        deviceActor1.Tell(new RecordTemperature(requestId: 0, value: 1.0), probe.Ref);
        probe.ExpectMsg<TemperatureRecorded>(s => s.RequestId == 0);
        deviceActor2.Tell(new RecordTemperature(requestId: 1, value: 2.0), probe.Ref);
        probe.ExpectMsg<TemperatureRecorded>(s => s.RequestId == 1);
        // No temperature for device3

        groupActor.Tell(new RequestAllTemperatures(0), probe.Ref);
        probe.ExpectMsg<RespondAllTemperatures>(msg =>
          msg.Temperatures["device1"].AsInstanceOf<Temperature>().Value == 1.0 &&
          msg.Temperatures["device2"].AsInstanceOf<Temperature>().Value == 2.0 &&
          msg.Temperatures["device3"] is TemperatureNotAvailable &&
          msg.RequestId == 0);
    }
}