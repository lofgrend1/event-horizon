using System.Text;

using HF.EventHorizon.Core.Enums;
using HF.EventHorizon.Core.Events;
using HF.EventHorizon.Core.Interfaces;

namespace HF.EventHorizon.File;

public class CSVPlugin : IProtocolPlugin, ISource, IDestination
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
        try
        {
            var filepath = additionalParameters.ContainsKey("filepath") ? additionalParameters["filepath"] : throw new Exception("Required Additional Parameter Not Found: ['filepath']");
            _connectionString = filepath;

            // check if the file already exists
            if (System.IO.File.Exists(_connectionString))
            {
                // read the first line of the file
                var firstLine = System.IO.File.ReadLines(_connectionString).FirstOrDefault();

                // if there are no headers, write them
                if (firstLine != "Date,Source,Address,Data")
                {
                    _fileStream = new FileStream(_connectionString, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    _streamWriter = new StreamWriter(_fileStream);
                    _streamWriter.WriteLine("Date,PluginId,Address,Data");
                }
                else
                {
                    _fileStream = new FileStream(_connectionString, FileMode.Append, FileAccess.Write, FileShare.None);
                    _streamWriter = new StreamWriter(_fileStream);
                }
            }
            else
            {
                _fileStream = new FileStream(_connectionString, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                _streamWriter = new StreamWriter(_fileStream);
                _streamWriter.WriteLine("Date,PluginId,Address,Data");
            }

            IsConnected = true;
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(IsConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected));
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));
        }
    }

    public async Task ConnectAsync(string connectionString, Dictionary<string, string> additionalParameters)
    {
        try
        {
            var filepath = additionalParameters.ContainsKey("filepath") ? additionalParameters["filepath"] : throw new Exception("Required Additional Parameter Not Found: ['filepath']");
            _connectionString = filepath;

            // check if the file already exists
            if (System.IO.File.Exists(_connectionString))
            {
                // read the first line of the file
                var firstLine = System.IO.File.ReadLines(_connectionString).FirstOrDefault();

                // if there are no headers, write them
                if (firstLine != "Date,Source,Address,Data")
                {
                    _fileStream = new FileStream(_connectionString, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    _streamWriter = new StreamWriter(_fileStream);
                    _streamWriter.WriteLine("Date,PluginId,Address,Data");
                }
                else
                {
                    _fileStream = new FileStream(_connectionString, FileMode.Append, FileAccess.Write, FileShare.None);
                    _streamWriter = new StreamWriter(_fileStream);
                }
            }
            else
            {
                _fileStream = new FileStream(_connectionString, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                _streamWriter = new StreamWriter(_fileStream);
                _streamWriter.WriteLine("Date,PluginId,Address,Data");
            }

            IsConnected = true;
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(IsConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected));
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));
        }
    }

    public void Disconnect()
    {
        try
        {
            _streamWriter.Close();
            _fileStream.Close();
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
            _streamWriter.Close();
            _fileStream.Close();
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
        _streamWriter?.Dispose();
        _fileStream?.Dispose();
    }

    public bool Publish(string address, byte[] data)
    {
        try
        {
            _streamWriter.WriteLine($"{DateTime.Now},{PluginId},{address},{Encoding.UTF8.GetString(data)}");
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
            await _streamWriter.WriteLineAsync($"{DateTime.Now},{PluginId},{address},{Encoding.UTF8.GetString(data)}");
            await _streamWriter.FlushAsync();
            return true;
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));
            return false;
        }
    }

    public byte[] ReadData(string address)
    {
        try
        {
            // Read the file and get lines with the specified address
            var lines = System.IO.File.ReadLines(_connectionString)
                            .Where(line => line.Split(',')[2].Trim() == address)
                            .ToList();

            // Convert the list of lines to bytes and return
            return Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines));
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));
            return null;
        }
    }

    public async Task<byte[]> ReadDataAsync(string address)
    {
        try
        {
            // Read the file and get lines with the specified address
            var lines = await System.IO.File.ReadAllLinesAsync(_connectionString);
            var matchingLines = lines.Where(line => line.Split(',')[2].Trim() == address).ToList();

            // Convert the list of lines to bytes and return
            return Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, matchingLines));
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));
            return null;
        }
    }
}
