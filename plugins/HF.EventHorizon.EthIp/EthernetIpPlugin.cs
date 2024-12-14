using libplctag;
using libplctag.DataTypes;

using HF.EventHorizon.Core.Enums;
using HF.EventHorizon.Core.Events;
using HF.EventHorizon.Core.Interfaces;

namespace HF.EventHorizon.EthIp;

public class EthernetIpPlugin : IProtocolPlugin, ISource, IDestination//, ISubscribable, IBrowsable
{
    #region Fields
    private string _connectionString;                    // ethernet/ip device connection string
    private Dictionary<string, string> _additionalParameters;   // Additional parameters for connection

    private PlcType _plcType;
    private Protocol _protocol;
    #endregion

    #region Properties
    public bool IsConnected { get; private set; }

    public string PluginId { get; set; }

    public string[] RequiredParameters => new string[]
    {
        "PlcType",
        "Protocol",
        "Gateway",
        "Path"
    };

    public List<string> Browse()
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> BrowseAsync()
    {
        throw new NotImplementedException();
    }

    public void Connect(string connectionString, Dictionary<string, string> additionalParameters)
    {
        _connectionString = connectionString;
        _additionalParameters = additionalParameters;

        try
        {
            var plcType = additionalParameters.ContainsKey("PlcType") ? additionalParameters["PlcType"] : throw new Exception("Required Additional Parameter Not Found: ['PlcType']");
            var protocol = additionalParameters.ContainsKey("Protocol") ? additionalParameters["Protocol"] : throw new Exception("Required Additional Parameter Not Found: ['Protocol']");

            var validPlcType = Enum.TryParse(plcType, out _plcType);
            if (!validPlcType)
                throw new Exception($"{plcType} is not a valid PlcType. " +
                    $"Must be {PlcType.ControlLogix}, {PlcType.Plc5}, {PlcType.Slc500}, {PlcType.LogixPccc}, {PlcType.Micro800}, {PlcType.MicroLogix}, {PlcType.Omron}");

            var validProtocol = Enum.TryParse(protocol, out _protocol);
            if (!validProtocol)
                throw new Exception($"{plcType} is not a valid Protocol. Must be either {Protocol.modbus_tcp} or {Protocol.ab_eip}");

            IsConnected = true;
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(IsConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected));
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));   // Trigger error event if an exception occurs
        }
    }

    public async Task ConnectAsync(string connectionString, Dictionary<string, string> additionalParameters)
    {
        _connectionString = connectionString;
        _additionalParameters = additionalParameters;

        try
        {
            var plcType = additionalParameters.ContainsKey("PlcType") ? additionalParameters["PlcType"] : throw new Exception("Required Additional Parameter Not Found: ['PlcType']");
            var protocol = additionalParameters.ContainsKey("Protocol") ? additionalParameters["Protocol"] : throw new Exception("Required Additional Parameter Not Found: ['Protocol']");

            var validPlcType = Enum.TryParse(plcType, out _plcType);
            if (!validPlcType)
                throw new Exception($"{plcType} is not a valid PlcType. " +
                    $"Must be {PlcType.ControlLogix}, {PlcType.Plc5}, {PlcType.Slc500}, {PlcType.LogixPccc}, {PlcType.Micro800}, {PlcType.MicroLogix}, {PlcType.Omron}");

            var validProtocol = Enum.TryParse(protocol, out _protocol);
            if (!validProtocol)
                throw new Exception($"{plcType} is not a valid Protocol. Must be either {Protocol.modbus_tcp} or {Protocol.ab_eip}");

            IsConnected = true;
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(IsConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected));
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));   // Trigger error event if an exception occurs
        }
    }

    public void Disconnect()
    {
        try
        {
            IsConnected = false;
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(IsConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected));
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            IsConnected = false;
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(IsConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected));
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));
        }
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public bool Publish(string address, byte[] data)
    {
        throw new NotImplementedException();
    }

    public Task<bool> PublishAsync(string address, byte[] data)
    {
        throw new NotImplementedException();
    }

    public byte[] ReadData(string address)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> ReadDataAsync(string address)
    {
        throw new NotImplementedException();
    }

    public void Subscribe(string address)
    {
        throw new NotImplementedException();
    }

    public Task SubscribeAsync(string address)
    {
        throw new NotImplementedException();
    }

    public void Unsubscribe(string address)
    {
        throw new NotImplementedException();
    }

    public Task UnsubscribeAsync(string address)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Events
    public event EventHandler<EvtHorizonDataReceivedEventArgs> DataReceived;   // Event triggered when data is received
    public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;   // Event triggered when connection state changes
    public event EventHandler<EvtHorizonErrorEventArgs> Error;   // Event triggered when an error occurs
    #endregion

    #region Private Methods

    private string ReadStringTag(string gateway, string path, string name, int timeoutSeconds = 5)
    {
        var tag = new Tag<StringPlcMapper, string>()
        {
            Name = name,
            Gateway = gateway,
            Path = path,
            PlcType = _plcType,
            Protocol = _protocol,
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };

        tag.Initialize();
        tag.Read();

        return tag.Value;
    }

    /// <summary>
    /// Read Bool Value from Controller
    /// </summary>
    /// <param name="gateway"></param>
    /// <param name="path"></param>
    /// <param name="name"></param>
    /// <param name="timeoutSeconds"></param>
    /// <returns></returns>
    private bool ReadBoolTag(string gateway, string path, string name, int timeoutSeconds = 5)
    {
        var tag = new Tag<BoolPlcMapper, bool>()
        {
            Name = name,
            Gateway = gateway,
            Path = path,
            PlcType = _plcType,
            Protocol = _protocol,
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };

        tag.Initialize();
        tag.Read();

        return tag.Value;
    }

    /// <summary>
    /// Read 8-bit Signed Number Value from Controller
    /// </summary>
    /// <param name="gateway"></param>
    /// <param name="path"></param>
    /// <param name="name"></param>
    /// <param name="timeoutSeconds"></param>
    /// <returns></returns>
    private sbyte ReadSintTag(string gateway, string path, string name, int timeoutSeconds = 5)
    {
        var tag = new Tag<SintPlcMapper, sbyte>()
        {
            Name = name,
            Gateway = gateway,
            Path = path,
            PlcType = _plcType,
            Protocol = _protocol,
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };

        tag.Initialize();
        tag.Read();

        return tag.Value;
    }

    /// <summary>
    /// Read 16-bit Signed Number Value from Controller
    /// </summary>
    /// <param name="gateway"></param>
    /// <param name="path"></param>
    /// <param name="name"></param>
    /// <param name="timeoutSeconds"></param>
    /// <returns></returns>
    private short ReadInt16Tag(string gateway, string path, string name, int timeoutSeconds = 5)
    {
        var tag = new Tag<IntPlcMapper, short>()
        {
            Name = name,
            Gateway = gateway,
            Path = path,
            PlcType = _plcType,
            Protocol = _protocol,
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };

        tag.Initialize();
        tag.Read();

        return tag.Value;
    }

    /// <summary>
    /// Read 32-bit Signed Number Value from Controller
    /// </summary>
    /// <param name="gateway"></param>
    /// <param name="path"></param>
    /// <param name="name"></param>
    /// <param name="timeoutSeconds"></param>
    /// <returns></returns>
    private int ReadInt32Tag(string gateway, string path, string name, int timeoutSeconds = 5)
    {
        var tag = new Tag<DintPlcMapper, int>()
        {
            Name = name,
            Gateway = gateway,
            Path = path,
            PlcType = _plcType,
            Protocol = _protocol,
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };

        tag.Initialize();
        tag.Read();

        return tag.Value;
    }

    /// <summary>
    /// Read 64-bit Signed Number Value from Controller
    /// </summary>
    /// <param name="gateway"></param>
    /// <param name="path"></param>
    /// <param name="name"></param>
    /// <param name="timeoutSeconds"></param>
    /// <returns></returns>
    private long ReadLintTag(string gateway, string path, string name, int timeoutSeconds = 5)
    {
        var tag = new Tag<LintPlcMapper, long>()
        {
            Name = name,
            Gateway = gateway,
            Path = path,
            PlcType = _plcType,
            Protocol = _protocol,
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };

        tag.Initialize();
        tag.Read();

        return tag.Value;
    }

    /// <summary>
    /// Read Floating Point Value from Controller
    /// </summary>
    /// <param name="gateway"></param>
    /// <param name="path"></param>
    /// <param name="name"></param>
    /// <param name="timeoutSeconds"></param>
    /// <returns></returns>
    private float ReadRealTag(string gateway, string path, string name, int timeoutSeconds = 5)
    {
        var tag = new Tag<RealPlcMapper, float>()
        {
            Name = name,
            Gateway = gateway,
            Path = path,
            PlcType = _plcType,
            Protocol = _protocol,
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };

        tag.Initialize();
        tag.Read();

        return tag.Value;
    }

    #endregion
}
