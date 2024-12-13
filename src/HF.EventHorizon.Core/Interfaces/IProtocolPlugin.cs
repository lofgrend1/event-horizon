using HF.EventHorizon.App.Models;

namespace HF.EventHorizon.Core.Interfaces;

/// <summary>
/// Defines a standard interface for protocol plugins.
/// </summary>
public interface IProtocolPlugin : IDisposable
{
    string[] RequiredParameters { get; }

    string PluginId { get; set; }

    /// <summary>
    /// Gets a value indicating whether the protocol plugin is connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Connects using the specified connection string and additional parameters.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="additionalParameters">A dictionary containing additional parameters.</param>
    void Connect(string connectionString, Dictionary<string, string> additionalParameters);

    /// <summary>
    /// Asynchronously connects using the specified connection string and additional parameters.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="additionalParameters">A dictionary containing additional parameters.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ConnectAsync(string connectionString, Dictionary<string, string> additionalParameters);

    /// <summary>
    /// Disconnects the protocol plugin.
    /// </summary>
    void Disconnect();

    /// <summary>
    /// Asynchronously disconnects the protocol plugin.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DisconnectAsync();

    /// <summary>
    /// Occurs when the connection state changes.
    /// </summary>
    event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

    /// <summary>
    /// Occurs when an error occurs.
    /// </summary>
    event EventHandler<EvtHorizonErrorEventArgs> Error;
}
