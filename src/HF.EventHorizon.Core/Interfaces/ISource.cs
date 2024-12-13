namespace HF.EventHorizon.Core.Interfaces;

/// <summary>
/// Defines the contract for a data source which can provide data synchronously and asynchronously.
/// </summary>
public interface ISource
{
    /// <summary>
    /// Reads and returns the data from the source at the given address synchronously.
    /// </summary>
    /// <param name="address">A string that specifies the location in the source from which to read the data.</param>
    /// <returns>A byte array that contains the data read from the source.</returns>
    byte[] ReadData(string address);

    /// <summary>
    /// Reads and returns the data from the source at the given address asynchronously.
    /// </summary>
    /// <param name="address">A string that specifies the location in the source from which to read the data.</param>
    /// <returns>A Task that represents the asynchronous operation. The task result contains a byte array that contains the data read from the source.</returns>
    Task<byte[]> ReadDataAsync(string address);
}
