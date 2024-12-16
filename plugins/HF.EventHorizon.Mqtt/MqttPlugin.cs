using System.Text;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using MQTTnet.Server;

using HF.EventHorizon.Core.Enums;
using HF.EventHorizon.Core.Events;
using HF.EventHorizon.Core.Interfaces;

namespace HF.EventHorizon.Mqtt;

/// <summary>
/// Represents an MQTT protocol plugin that provides connectivity and messaging functionality.
/// </summary>
public class MqttPlugin : IProtocolPlugin, IDestination, ISubscribable
{
    #region Fields
    private IMqttClient _client = null!;                         // MQTT client instance
    private MqttClientOptions _options = null!;                  // MQTT client options for connection configuration
    private string _connectionString = null!;                    // MQTT broker connection string
    private Dictionary<string, string> _additionalParameters = [];   // Additional parameters for connection

    #endregion

    #region Properties
    public bool IsConnected => _client?.IsConnected ?? false;   // Property indicating whether the client is connected
    public string PluginId { get; set; } = null!;                // Plugin ID

    public string[] RequiredParameters => new string[]
    {
        "Port",
        "Username",
        "Password"
    };
    #endregion

    #region Events
    public event EventHandler<EvtHorizonDataReceivedEventArgs> DataReceived = delegate { };             // Event triggered when data is received
    public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged = delegate { };   // Event triggered when connection state changes
    public event EventHandler<EvtHorizonErrorEventArgs> Error = delegate { };                           // Event triggered when an error occurs
    #endregion

    #region Public Methods

    /// <summary>
    /// Asynchronously connects to the MQTT broker using the specified connection string and additional parameters.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="additionalParameters">A dictionary containing additional parameters.</param>
    public async Task ConnectAsync(string connectionString, Dictionary<string, string> additionalParameters)
    {
        _connectionString = connectionString;
        _additionalParameters = additionalParameters;

        try
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();   // Create MQTT client instance

            // Build MQTT client options for connection
            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())   // Generate a unique client ID
                .WithTcpServer(_connectionString, int.Parse(_additionalParameters["Port"]));   // Set broker TCP server address and port

            // Conditionally add credentials if valid
            if (_additionalParameters.ContainsKey("Username") && !string.IsNullOrEmpty(_additionalParameters["Username"])
                && _additionalParameters.ContainsKey("Password") && !string.IsNullOrEmpty(_additionalParameters["Password"]))
            {
                optionsBuilder = optionsBuilder.WithCredentials(_additionalParameters["Username"], _additionalParameters["Password"]);
            }

            _options = optionsBuilder.Build();   // Build the MQTT client options

            // Setup message handling before connecting so that queued messages are also handled properly.
            _client.ApplicationMessageReceivedAsync += onApplicationMessageReceivedAsync;
            _client.ConnectingAsync += onClientConnectingAsync;
            _client.ConnectedAsync += onClientConnectedAsync;

            _client.DisconnectedAsync += async e =>
            {
                Console.WriteLine("Client Disconnected.");
                //e.DumpToConsole();   // Dump disconnection event details to console for debugging

                ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionStatus.Disconnected, e.Exception?.Message));

                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await _client.ConnectAsync(_options, CancellationToken.None);   // Reconnect to the broker
                }
                catch
                {
                    Console.WriteLine("Reconnection failed.");
                }
            };

            var response = await _client.ConnectAsync(_options, CancellationToken.None);   // Connect to the MQTT broker

            Console.WriteLine("The MQTT client is connected.");
            //response.DumpToConsole();   // Dump connection response details to console for debugging

            await SubscribeToTopics(additionalParameters, factory);
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));   // Trigger error event if an exception occurs
        }
    }

    /// <summary>
    /// Asynchronously disconnects from the MQTT broker.
    /// </summary>
    public async Task DisconnectAsync()
    {
        try
        {
            var factory = new MqttFactory();

            // Send a clean disconnect to the server by calling DisconnectAsync.
            // Without this, the TCP connection gets dropped and the server will handle this as a non-clean disconnect.
            var mqttClientDisconnectOptions = factory.CreateClientDisconnectOptionsBuilder().Build();

            await _client.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);   // Disconnect from the MQTT broker
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));   // Trigger error event if an exception occurs
        }
    }

    /// <summary>
    /// Asynchronously subscribes to the specified address.
    /// </summary>
    /// <param name="address">The address to subscribe to.</param>
    public async Task SubscribeAsync(string address)
    {
        try
        {
            await _client.SubscribeAsync(new MqttTopicFilter { Topic = address });   // Subscribe to the specified topic
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));   // Trigger error event if an exception occurs
        }
    }

    /// <summary>
    /// Asynchronously unsubscribes from the specified address.
    /// </summary>
    /// <param name="address">The address to unsubscribe from.</param>
    public async Task UnsubscribeAsync(string address)
    {
        try
        {
            await _client.UnsubscribeAsync(address);   // Unsubscribe from the specified topic
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));   // Trigger error event if an exception occurs
        }
    }

    /// <summary>
    /// Asynchronously publishes the specified data to the specified address.
    /// </summary>
    /// <param name="address">The address to which the data is published.</param>
    /// <param name="data">The data to be published.</param>
    /// <returns>A task representing the asynchronous operation and a value indicating whether the publish operation was successful.</returns>
    public async Task<bool> PublishAsync(string address, byte[] data)
    {
        try
        {
            var message = new MqttApplicationMessage
            {
                Topic = address,              // Set the topic of the message
                PayloadSegment = data         // Set the payload of the message
            };

            await _client.PublishAsync(message);   // Publish the message to the MQTT broker

            return true;
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));   // Trigger error event if an exception occurs
            return false;
        }
    }

    /// <summary>
    /// Synchronously connects to the MQTT broker using the specified connection string and additional parameters.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="additionalParameters">A dictionary containing additional parameters.</param>
    public void Connect(string connectionString, Dictionary<string, string> additionalParameters)
    {
        ConnectAsync(connectionString, additionalParameters).Wait();
    }

    /// <summary>
    /// Synchronously disconnects from the MQTT broker.
    /// </summary>
    public void Disconnect()
    {
        DisconnectAsync().Wait();
    }

    /// <summary>
    /// Synchronously subscribes to the specified address.
    /// </summary>
    /// <param name="address">The address to subscribe to.</param>
    public void Subscribe(string address)
    {
        SubscribeAsync(address).Wait();
    }

    /// <summary>
    /// Synchronously unsubscribes from the specified address.
    /// </summary>
    /// <param name="address">The address to unsubscribe from.</param>
    public void Unsubscribe(string address)
    {
        UnsubscribeAsync(address).Wait();
    }

    /// <summary>
    /// Synchronously publishes the specified data to the specified address.
    /// </summary>
    /// <param name="address">The address to which the data is published.</param>
    /// <param name="data">The data to be published.</param>
    /// <returns>A value indicating whether the publish operation was successful.</returns>
    public bool Publish(string address, byte[] data)
    {
        return PublishAsync(address, data).Result;
    }

    public void Dispose()
    {
        // Disconnect the client from the server.
        // Since DisconnectAsync is an async method, we need to wait for it to finish.
        // Make sure that it doesn't throw exceptions, because Dispose should never throw exceptions.
        try
        {
            DisconnectAsync().Wait();
        }
        catch
        {
            // Ignore any exceptions, as Dispose should never throw exceptions.
        }

        // Dispose of the client.
        _client?.Dispose();
    }

    #endregion

    #region Private Methods

    private Task onClientConnectingAsync(MqttClientConnectingEventArgs arg)
    {
        Console.WriteLine("Client Connecting.");
        //arg.DumpToConsole();   // Dump connection event details to console for debugging

        ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionStatus.Connecting));

        // Now the broker will resend the message again.
        return Task.CompletedTask;
    }

    private Task onApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        Console.WriteLine("Received application message.");

        var message = Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment);
        DataReceived?.Invoke(this, new EvtHorizonDataReceivedEventArgs(arg.ApplicationMessage.Topic, Encoding.UTF8.GetBytes(message)));

        // Now the broker will resend the message again.
        return Task.CompletedTask;
    }

    private Task onClientConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        Console.WriteLine("Client Connected.");
        //arg.DumpToConsole();   // Dump connection event details to console for debugging

        ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionStatus.Connected));

        // Now the broker will resend the message again.
        return Task.CompletedTask;
    }

    private async Task SubscribeToTopics(Dictionary<string, string> additionalParameters, MqttFactory factory)
    {
        // Get all of the topics to subscribe to
        var mqttSubscribeOptionsBuilder = factory.CreateSubscribeOptionsBuilder();

        foreach (var param in additionalParameters)
            if (param.Key.ToLowerInvariant().StartsWith("topic")) mqttSubscribeOptionsBuilder.WithTopicFilter(f => f.WithTopic(param.Value));

        var mqttSubscribeOptions = mqttSubscribeOptionsBuilder.Build();

        await _client.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);   // Subscribe to topics
    }

    #endregion
}