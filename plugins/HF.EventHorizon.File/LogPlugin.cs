using System.Text;

using HF.EventHorizon.Core.Enums;
using HF.EventHorizon.Core.Events;
using HF.EventHorizon.Core.Interfaces;

namespace HF.EventHorizon.File;

public class LogPlugin : IProtocolPlugin, IDestination
{
    private FileStream _fileStream;
    private StreamWriter _streamWriter;
    private string _connectionString;

    public bool IsConnected { get; private set; }
    public string[] RequiredParameters => new string[] { "filepath" };

    public string PluginId { get; set; }

    public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

    public event EventHandler<EvtHorizonErrorEventArgs> Error;

    public void Connect(string connectionString, Dictionary<string, string> additionalParameters)
    {
        var filepath = additionalParameters.ContainsKey("filepath") ? additionalParameters["filepath"] : throw new Exception("Required Additional Parameter Not Found: ['filepath']");
        _connectionString = filepath;

        _fileStream = new FileStream(_connectionString, FileMode.Append, FileAccess.Write, FileShare.None);
        _streamWriter = new StreamWriter(_fileStream);

        IsConnected = true;
        ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(IsConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected));
    }

    public async Task ConnectAsync(string connectionString, Dictionary<string, string> additionalParameters)
    {
        var filepath = additionalParameters.ContainsKey("filepath") ? additionalParameters["filepath"] : throw new Exception("Required Additional Parameter Not Found: ['filepath']");
        _connectionString = filepath;

        _fileStream = new FileStream(_connectionString, FileMode.Append, FileAccess.Write, FileShare.None);
        _streamWriter = new StreamWriter(_fileStream);

        IsConnected = true;
        ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(IsConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected));

        await Task.CompletedTask;
    }

    public void Disconnect()
    {
        _streamWriter.Close();
        _fileStream.Close();
        IsConnected = false;
        ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(IsConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected));
    }

    public async Task DisconnectAsync()
    {
        _streamWriter.Close();
        _fileStream.Close();
        IsConnected = false;
        ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(IsConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected));

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _streamWriter?.Dispose();
        _fileStream?.Dispose();
    }

    public bool Publish(string address, byte[] data)
    {
        try
        {
            string logMessage = Encoding.UTF8.GetString(data);
            _streamWriter.WriteLine($"[{DateTime.Now}] {PluginId}.{address}: {logMessage}");
            _streamWriter.Flush();
            return true;
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));
            return false;
        }
    }

    public async Task<bool> PublishAsync(string address, byte[] data)
    {
        try
        {
            string logMessage = Encoding.UTF8.GetString(data);
            await _streamWriter.WriteLineAsync($"[{DateTime.Now}] {PluginId}.{address}: {logMessage}");
            await _streamWriter.FlushAsync();
            return true;
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));
            return false;
        }
    }
}
