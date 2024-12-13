namespace HF.EventHorizon.App.Models;

/// <summary>
/// Event arguments for an error event. This class contains the error message, the time when the error occurred and additional details.
/// </summary>
public class EvtHorizonErrorEventArgs : EventArgs
{
    /// <summary>
    /// Gets the error message describing the error.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Gets the time when the error occurred.
    /// </summary>
    public DateTime ErrorTimeStamp { get; }

    /// <summary>
    /// Gets the address or identifier associated with the error, such as a topic or a data point identifier.
    /// </summary>
    public string Address { get; }

    /// <summary>
    /// Gets the error code, if available.
    /// </summary>
    public string ErrorCode { get; }

    public EvtHorizonErrorEventArgs(string errorMessage, string? address = null, string? errorCode = null)
    {
        var defaultString = "<EMPTY>";

        ErrorMessage = errorMessage ?? string.Empty;
        ErrorTimeStamp = DateTime.UtcNow;
        Address = address ?? defaultString;
        ErrorCode = errorCode ?? defaultString;
    }
}
