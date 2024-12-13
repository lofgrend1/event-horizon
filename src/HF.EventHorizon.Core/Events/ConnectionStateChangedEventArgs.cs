using HF.EventHorizon.Core.Enums;

namespace HF.EventHorizon.Core.Events;

/// <summary>
/// Event arguments for the ConnectionStateChanged event.
/// </summary>
public class ConnectionStateChangedEventArgs : EventArgs
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
    public ConnectionStateChangedEventArgs(ConnectionStatus status, string? message = null)
    {
        var defaultString = "<EMPTY>";

        Status = status;
        Message = message ?? defaultString;
        TimeStamp = DateTime.UtcNow;
    }
}
