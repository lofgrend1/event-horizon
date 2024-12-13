using HF.EventBus.Abstractions;

namespace HF.EventBus.Events;

public class ServiceStateChangedIntegrationEvent : SerializedMessage
{
    public string DeviceId { get; set; }
    public string State { get; set; }
    public string ServiceId { get; set; }

    public ServiceStateChangedIntegrationEvent(string deviceId, string serviceId,
        string state, string? instanceId = null) : base(instanceId ?? GenerateInstanceId())
    {
        ServiceId = serviceId;
        State = state ?? throw new ArgumentNullException("state");
        DeviceId = deviceId ?? throw new ArgumentNullException("DeviceId");
    }

    private static string GenerateInstanceId()
    {
        return Guid.NewGuid().ToString();
    }
}