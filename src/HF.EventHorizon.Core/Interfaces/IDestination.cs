namespace HF.EventHorizon.Core.Interfaces;

public interface IDestination
{
    /// <summary>
    /// Publishes the specified data to the specified address.
    /// </summary>
    /// <param name="address">The address to which the data is published.</param>
    /// <param name="data">The data to be published.</param>
    /// <returns>A value indicating whether the publish operation was successful.</returns>
    bool Publish(string address, byte[] data);

    /// <summary>
    /// Asynchronously publishes the specified data to the specified address.
    /// </summary>
    /// <param name="address">The address to which the data is published.</param>
    /// <param name="data">The data to be published.</param>
    /// <returns>A task representing the asynchronous operation and a value indicating whether the publish operation was successful.</returns>
    Task<bool> PublishAsync(string address, byte[] data);
}
