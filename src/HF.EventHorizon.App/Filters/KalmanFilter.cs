using HF.EventHorizon.App.Interfaces;
using HF.EventHorizon.Core.Events;

namespace HF.EventHorizon.App.Filters;

public class KalmanFilter : IRoutingFilterPlugin, IBufferedFilter
{
    private float estimated;
    private float error;
    private float Q; // Process noise
    private float R; // Measurement noise

    private Queue<double> bufferData;
    public int BufferSize { get; set; }
    public Queue<double> BufferData => bufferData;

    public bool IsReady { get; private set; }

    public string[] RequiredParameters => new string[]
    {
        "InitialEstimated",
        "InitialError",
        "ProcessNoise",
        "MeasurementNoise",
        "BufferSize"
    };

    public string PluginId { get; set; } = string.Empty;

    public event EventHandler<EvtHorizonErrorEventArgs> Error = delegate { };

    public KalmanFilter()
    {
        this.bufferData = new Queue<double>();
        this.IsReady = false;
    }

    public void Configure(Dictionary<string, string> parameters)
    {
        try
        {
            this.estimated = float.Parse(parameters["InitialEstimated"]);
            this.error = float.Parse(parameters["InitialError"]);
            this.Q = float.Parse(parameters["ProcessNoise"]);
            this.R = float.Parse(parameters["MeasurementNoise"]);
            this.BufferSize = int.Parse(parameters["BufferSize"]);
            this.IsReady = true;
        }
        catch (Exception ex)
        {
            this.IsReady = false;
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));
        }
    }

    public async Task ConfigureAsync(Dictionary<string, string> parameters)
    {
        await Task.Run(() => Configure(parameters));
    }

    public byte[] FilterData(byte[] data)
    {
        double newData = BitConverter.ToDouble(data, 0);

        // Update buffer and perform Kalman filtering
        UpdateBuffer(newData);
        float filteredData = ApplyFilter((float)newData);

        // Convert filtered data back to byte[] and return
        return BitConverter.GetBytes(filteredData);
    }

    public async Task<byte[]> FilterDataAsync(byte[] data)
    {
        return await Task.Run(() => FilterData(data));
    }

    public void UpdateBuffer(double newData)
    {
        if (bufferData.Count >= BufferSize)
        {
            bufferData.Dequeue();
        }
        bufferData.Enqueue(newData);
    }

    public void ClearBuffer()
    {
        bufferData.Clear();
    }

    private float ApplyFilter(float current_measurement)
    {
        // Prediction step (we assume no control input, so no need for control matrices)
        float predicted_estimated = estimated;
        float predicted_error = error + Q;

        // Update step
        float kalman_gain = predicted_error / (predicted_error + R);
        estimated = predicted_estimated + kalman_gain * (current_measurement - predicted_estimated);
        error = (1 - kalman_gain) * predicted_error;

        return estimated;
    }

    public void Dispose()
    {
        // Free any resources if needed
    }
}
