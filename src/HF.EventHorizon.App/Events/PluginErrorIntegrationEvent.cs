using HF.EventBus.Events;

namespace HF.EventHorizon.App.Events;

public record PluginErrorIntegrationEvent : IntegrationEvent
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

    public PluginErrorIntegrationEvent(string errorMessage, string address = null!, string errorCode = null!)
    {
        ErrorMessage = errorMessage;
        ErrorTimeStamp = DateTime.UtcNow;
        Address = address;
        ErrorCode = errorCode;
    }
}