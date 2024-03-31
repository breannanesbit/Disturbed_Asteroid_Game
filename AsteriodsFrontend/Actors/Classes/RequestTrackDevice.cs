using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.Classes;
public sealed class RequestTrackDevice
{
    public RequestTrackDevice(string groupid, string deviceId)
    {
        Groupid = groupid;
        DeviceId = deviceId;
    }
    public string Groupid { get; }
    public string DeviceId { get; }
}
public sealed class DeviceRegistered
{
    public static DeviceRegistered Instance { get; } = new();
    private DeviceRegistered() { }
}
