namespace HF.EventHorizon.Core.Events;

/// <summary>
/// Event arguments for the DataReceived event. This class contains the data received, the address from which the data originated, and the time when the data was received.
/// </summary>
public class EvtHorizonDataReceivedEventArgs : EventArgs
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

    public EvtHorizonDataReceivedEventArgs(string? address, byte[] data)
    {
        Address = address;
        Data = data;
        TimeStamp = DateTime.UtcNow;
    }
}
