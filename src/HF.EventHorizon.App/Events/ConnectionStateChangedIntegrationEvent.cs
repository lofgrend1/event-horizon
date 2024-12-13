using HF.EventBus.Events;
using HF.EventHorizon.Core.Enums;

namespace HF.EventHorizon.App.Events;

public record ConnectionStateChangedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// Gets the new connection status.
    /// </summary>
    public ConnectionStatus Status { get; }

    /// <summary>
    /// Gets the timestamp when the status change occurred.
    /// </summary>
    public DateTime TimeStamp { get; }

    /// <summary>
    /// Gets an optional message associated with the status change, such as an error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Initializes a new instance of the ConnectionStateChangedEventArgs class.
    /// </summary>
    /// <param name="status">The new connection status.</param>
    /// <param name="message">An optional message associated with the status change.</param>
    public ConnectionStateChangedIntegrationEvent(ConnectionStatus status, string message = null!)
    {
        Status = status;
        Message = message;
        TimeStamp = DateTime.UtcNow;
    }
}