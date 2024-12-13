namespace HF.EventHorizon.App.Interfaces;

public interface IBufferedFilter
{
    /// <summary>
    /// Gets or sets the size of the buffer.
    /// </summary>
    int BufferSize { get; set; }

    /// <summary>
    /// Gets the current buffer data.
    /// </summary>
    Queue<double> BufferData { get; }

    /// <summary>
    /// Updates the buffer with the latest data.
    /// </summary>
    /// <param name="newData">The new data to be added to the buffer.</param>
    void UpdateBuffer(double newData);

    /// <summary>
    /// Clears the buffer.
    /// </summary>
    void ClearBuffer();
}