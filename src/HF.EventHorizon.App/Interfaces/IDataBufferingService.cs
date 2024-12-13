namespace HF.EventHorizon.App.Interfaces;

public interface IDataBufferingService
{
    int BufferCapacity { get; }

    /// <summary>
    /// Initialize the service with a specific buffer capacity.
    /// </summary>
    /// <param name="capacity">The maximum number of data records each buffer can hold.</param>
    void Initialize(int capacity);

    /// <summary>
    /// Handles the receipt of data from a data source. If a buffer for the source's address
    /// does not exist, it is created. If the buffer is at capacity, the oldest data record
    /// is removed before the new record is added.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The data event args, which include the data source's address and the new data record.</param>
    void AddToBuffer(string address, byte[] data);

    /// <summary>
    /// Get the buffered data for a specific address.
    /// </summary>
    /// <param name="address">The address of the data source.</param>
    /// <returns>The buffered data for the address, or null if no buffer exists for the address.</returns>
    Queue<byte[]>? GetBufferData(string address);
}
