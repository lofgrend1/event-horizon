using HF.EventHorizon.Core.Events;

namespace HF.EventHorizon.Core.Interfaces;

public interface ISubscribable
{
    /// <summary>
    /// Subscribes to the specified address.
    /// </summary>
    /// <param name="address">The address to subscribe to.</param>
    void Subscribe(string address);

    /// <summary>
    /// Asynchronously subscribes to the specified address.
    /// </summary>
    /// <param name="address">The address to subscribe to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SubscribeAsync(string address);

    /// <summary>
    /// Unsubscribes from the specified address.
    /// </summary>
    /// <param name="address">The address to unsubscribe from.</param>
    void Unsubscribe(string address);

    /// <summary>
    /// Asynchronously unsubscribes from the specified address.
    /// </summary>
    /// <param name="address">The address to unsubscribe from.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UnsubscribeAsync(string address);

    /// <summary>
    /// Occurs when data is received.
    /// </summary>
    event EventHandler<EvtHorizonDataReceivedEventArgs> DataReceived;
}
