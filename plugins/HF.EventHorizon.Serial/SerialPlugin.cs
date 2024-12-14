using System.IO.Ports;

using HF.EventHorizon.Core.Enums;
using HF.EventHorizon.Core.Events;
using HF.EventHorizon.Core.Interfaces;

namespace HF.EventHorizon.Serial;

public class SerialPlugin : IProtocolPlugin, ISource, IDestination
{
    private SerialPort _serialPort;

    public string PluginId { get; set; }

    public bool IsConnected => _serialPort != null && _serialPort.IsOpen;

    public string[] RequiredParameters => new[] { "COM", "BaudRate", "Parity", "DataBits", "StopBits" };

    public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;
    public event EventHandler<EvtHorizonErrorEventArgs> Error;

    public void Connect(string connectionString, Dictionary<string, string> additionalParameters)
    {
        try
        {
            string portName = additionalParameters["COM"];
            int baudRate = int.Parse(additionalParameters["BaudRate"]);
            Parity parity = (Parity)Enum.Parse(typeof(Parity), additionalParameters["Parity"]);
            int dataBits = int.Parse(additionalParameters["DataBits"]);
            StopBits stopBits = (StopBits)Enum.Parse(typeof(StopBits), additionalParameters["StopBits"]);

            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            _serialPort.Open();

            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionStatus.Connected));
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));
        }
    }

    public Task ConnectAsync(string connectionString, Dictionary<string, string> additionalParameters)
    {
        return Task.Run(() => Connect(connectionString, additionalParameters));
    }

    public void Disconnect()
    {
        try
        {
            _serialPort?.Close();
            _serialPort = null;

            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionStatus.Disconnected));
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));
        }
    }

    public Task DisconnectAsync()
    {
        return Task.Run(() => Disconnect());
    }

    public void Dispose()
    {
        Disconnect();
        _serialPort?.Dispose();
    }

    public bool Publish(string address, byte[] data)
    {
        try
        {
            _serialPort.Write(data, 0, data.Length);
            return true;
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));
            return false;
        }
    }

    public Task<bool> PublishAsync(string address, byte[] data)
    {
        return Task.Run(() => Publish(address, data));
    }

    public byte[] ReadData(string address)
    {
        try
        {
            byte[] buffer = new byte[_serialPort.BytesToRead];
            _serialPort.Read(buffer, 0, buffer.Length);
            return buffer;
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));
            return null;
        }
    }

    public Task<byte[]> ReadDataAsync(string address)
    {
        return Task.Run(() => ReadData(address));
    }
}
