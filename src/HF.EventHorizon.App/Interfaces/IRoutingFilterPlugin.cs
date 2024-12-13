using HF.EventHorizon.Core.Events;

namespace HF.EventHorizon.App.Interfaces;

public interface IRoutingFilterPlugin : IDisposable
{
    string[] RequiredParameters { get; }

    string PluginId { get; set; }

    /// <summary>
    /// Gets a value indicating whether the filter plugin is initialized and ready to process data.
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// Configures the filter plugin with the given parameters.
    /// </summary>
    /// <param name="parameters">A dictionary containing the configuration parameters for the filter.</param>
    void Configure(Dictionary<string, string> parameters);

    /// <summary>
    /// Asynchronously configures the filter plugin with the given parameters.
    /// </summary>
    /// <param name="parameters">A dictionary containing the configuration parameters for the filter.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ConfigureAsync(Dictionary<string, string> parameters);

    /// <summary>
    /// Filters the given data.
    /// </summary>
    /// <param name="data">The data to be filtered.</param>
    /// <returns>The filtered data.</returns>
    byte[] FilterData(byte[] data);

    /// <summary>
    /// Asynchronously filters the given data.
    /// </summary>
    /// <param name="data">The data to be filtered.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the filtered data.</returns>
    Task<byte[]> FilterDataAsync(byte[] data);

    /// <summary>
    /// Occurs when an error occurs.
    /// </summary>
    event EventHandler<EvtHorizonErrorEventArgs> Error;
}
