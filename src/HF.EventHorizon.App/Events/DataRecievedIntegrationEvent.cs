using HF.EventBus.Events;

namespace HF.EventHorizon.App.Events;

public record DataRecievedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// Gets or sets the address (such as a topic or a data point identifier) from which the data was received.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the data that was received.
    /// </summary>
    public byte[] Data { get; set; }

    /// <summary>
    /// Gets the time when the data was received.
    /// </summary>
    public DateTime TimeStamp { get; }

    public DataRecievedIntegrationEvent(string? address, byte[] data)
    {
        Address = address;
        Data = data;
        TimeStamp = DateTime.UtcNow;
    }
}