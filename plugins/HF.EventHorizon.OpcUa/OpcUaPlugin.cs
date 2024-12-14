using System.Text;

using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

using HF.EventHorizon.Core.Enums;
using HF.EventHorizon.Core.Events;
using HF.EventHorizon.Core.Interfaces;

namespace HF.EventHorizon.OpcUa;

/// <summary>
/// Implements the OPC UA protocol plugin, enabling connection, disconnection, and subscription to an OPC UA server.
/// </summary>
public class OpcUaPlugin : IProtocolPlugin, ISource, ISubscribable, IBrowsable//, IDestination
{
    #region Fields
    private Session _session;                                 // OPC UA session instance
    private SessionReconnectHandler _reconnectHandler;
    private string _connectionString;                         // OPC UA server connection string
    private Dictionary<string, string> _additionalParameters; // Additional parameters for connection
    private bool _autoAccept = false;
    private readonly int _reconnectPeriod = 10;
    private readonly int _publishingInterval = 1000;
    #endregion

    #region Properties
    /// <summary>
    /// Checks whether the client is currently connected to the OPC UA server.
    /// </summary>
    public bool IsConnected => _session?.Connected ?? false;  // Property indicating whether the client is connected
    public string PluginId { get; set; }

    public string[] RequiredParameters => new string[]
    {
        "ClientName",
        "ReconnectPeriod",
        "Username",
        "Password",
        "PublishingInterval",
        "ConfigurationPath",
        "ConfigSectionName",
        "ApplicationCertificatePath",
        "PrivateKeyPath",
        "SessionTimeout",
        "EndpointURL",
        "OperationTimeout",
        "MaxBufferSize",
        "MaxMessageSize"
    };
    #endregion

    #region Events
    public event EventHandler<EvtHorizonDataReceivedEventArgs> DataReceived;  // Event triggered when data is received
    public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;  // Event triggered when connection state changes
    public event EventHandler<EvtHorizonErrorEventArgs> Error;  // Event triggered when an error occurs
    #endregion

    #region Public Methods
    /// <summary>
    /// Connects to the OPC UA server using provided connection string and additional parameters.
    /// </summary>
    public void Connect(string connectionString, Dictionary<string, string> additionalParameters)
    {
        ConnectAsync(connectionString, additionalParameters).Wait();
    }

    /// <summary>
    /// Asynchronously connects to the OPC UA server using provided connection string and additional parameters.
    /// </summary>
    public async Task ConnectAsync(string connectionString, Dictionary<string, string> additionalParameters)
    {
        _connectionString = connectionString;
        _additionalParameters = additionalParameters;
        _reconnectHandler = new SessionReconnectHandler();

        try
        {
            var appName = _additionalParameters.ContainsKey("ClientName") ? _additionalParameters["ClientName"] : @"DefaultClientName";
            var application = new ApplicationInstance
            {
                ApplicationName = appName,
                ApplicationType = ApplicationType.Client,
                ConfigSectionName = _additionalParameters.ContainsKey("ConfigSectionName") ? _additionalParameters["ConfigSectionName"] : @"Default"
            };

            var configPath = _additionalParameters.ContainsKey("ConfigurationPath") ? _additionalParameters["ConfigurationPath"] : @"opcua.config.xml";
            // load the application configuration.
            var config = await application.LoadApplicationConfiguration(configPath, false);
            //var config = new ApplicationConfiguration()
            //{
            //    ApplicationName = appName,
            //    ApplicationType = ApplicationType.Client,
            //    SecurityConfiguration = new SecurityConfiguration
            //    {
            //        ApplicationCertificate = new CertificateIdentifier
            //        {
            //            StoreType = "Directory",
            //            StorePath = _additionalParameters.ContainsKey("ApplicationCertificatePath") ? _additionalParameters["ApplicationCertificatePath"] : @"./CertificateStore/MyApplication",
            //            SubjectName = appName
            //        },
            //        TrustedPeerCertificates = new CertificateTrustList { StoreType = "Directory", StorePath = @"./CertificateStore/UA Applications" },
            //        RejectedCertificateStore = new CertificateTrustList { StoreType = "Directory", StorePath = @"./CertificateStore/RejectedCertificates" },
            //        AutoAcceptUntrustedCertificates = true
            //    },
            //    TransportConfigurations = new TransportConfigurationCollection(),
            //    TransportQuotas = new TransportQuotas { OperationTimeout = _additionalParameters.ContainsKey("OperationTimeout") ? int.Parse(_additionalParameters["OperationTimeout"]) : 60000 },
            //    ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = _additionalParameters.ContainsKey("SessionTimeout") ? int.Parse(_additionalParameters["SessionTimeout"]) : 60000 },
            //    TraceConfiguration = new TraceConfiguration()
            //};
            await config.Validate(ApplicationType.Client);

            bool haveAppCertificate = await application.CheckApplicationInstanceCertificate(false, 0);
            // check the application certificate.
            if (!haveAppCertificate)
                throw new AppCertificateNotProvidedException("Application instance certificate invalid!");

            config.ApplicationUri =
                X509Utils.GetApplicationUriFromCertificate(config.SecurityConfiguration.ApplicationCertificate
                    .Certificate);

            if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                _autoAccept = true;

            config.CertificateValidator.CertificateValidation +=
                new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);

            var endpointUrl = _additionalParameters.ContainsKey("EndpointURL") ? _additionalParameters["EndpointURL"] : throw new Exception("Invalid Endpoint Provided in Additional Parameters");

            var selectedEndpoint = CoreClientUtils.SelectEndpoint(endpointUrl, haveAppCertificate, 15000);

            var endpointConfiguration = EndpointConfiguration.Create(config);
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);

            // Connect to the server
            //_session = await Session.Create(config, endpoint, false, appName, 60000, new UserIdentity(host.Username, host.Password), null);
            _session = await Session.Create(config, endpoint, false, appName, 60000, new UserIdentity(new AnonymousIdentityToken()), null);

            Console.WriteLine("The OPC UA client is connected.");
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionStatus.Connected, null));

            // Start monitoring the specified tags
            var subscription = CreateSubscription(_session);

            _session.AddSubscription(subscription);
            subscription.Create();

            foreach (var pair in _additionalParameters)
            {
                if (pair.Key.StartsWith("tag"))
                {
                    var tagAddress = pair.Value;

                    var monitoredItem = new MonitoredItem(subscription.DefaultItem)
                    {
                        DisplayName = tagAddress,
                        StartNodeId = ExpandedNodeId.ToNodeId(tagAddress, _session.NamespaceUris),
                        AttributeId = Attributes.Value,
                        SamplingInterval = 1000,
                        QueueSize = 0,
                        DiscardOldest = true
                    };

                    monitoredItem.Notification += OnMonitoredItemNotification;
                    subscription.AddItem(monitoredItem);  // Add item to the created subscription, not the session's default subscription
                }
            }

            subscription.ApplyChanges();

            // register keep alive handler
            _session.KeepAlive += Client_KeepAlive; ;
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));   // Trigger error event if an exception occurs
        }
    }

    /// <summary>
    /// Disconnects from the OPC UA server.
    /// </summary>
    public void Disconnect()
    {
        DisconnectAsync().Wait();
    }

    /// <summary>
    /// Asynchronously disconnects from the OPC UA server.
    /// </summary>
    public async Task DisconnectAsync()
    {
        await _session.CloseAsync();
    }

    /// <summary>
    /// Subscribes to updates from a specific address in the OPC UA server.
    /// </summary>
    public void Subscribe(string address)
    {
        SubscribeAsync(address).Wait();
    }

    /// <summary>
    /// Asynchronously subscribes to updates from a specific address in the OPC UA server.
    /// </summary>
    public async Task SubscribeAsync(string address)
    {
        var subscription = _session.Subscriptions.FirstOrDefault();

        var monitoredItem = new MonitoredItem(subscription.DefaultItem)
        {
            DisplayName = address,
            StartNodeId = ExpandedNodeId.ToNodeId(address, _session.NamespaceUris),
            AttributeId = Attributes.Value,
            SamplingInterval = 1000,
            QueueSize = 0,
            DiscardOldest = true
        };

        monitoredItem.Notification += OnMonitoredItemNotification;
        subscription.AddItem(monitoredItem);  // Add item to the created subscription, not the session's default subscription

        await subscription.ApplyChangesAsync();
    }

    /// <summary>
    /// Unsubscribes from updates from a specific address in the OPC UA server.
    /// </summary>
    public void Unsubscribe(string address)
    {
        UnsubscribeAsync(address).Wait();
    }

    /// <summary>
    /// Asynchronously unsubscribes from updates from a specific address in the OPC UA server.
    /// </summary>
    public async Task UnsubscribeAsync(string address)
    {
        // TODO: Implement unsubscription from OPC UA server
    }

    /// <summary>
    /// Publishes data to a specific address in the OPC UA server.
    /// </summary>
    public bool Publish(string address, byte[] data)
    {
        return PublishAsync(address, data).Result;
    }

    /// <summary>
    /// Asynchronously publishes data to a specific address in the OPC UA server.
    /// </summary>
    public async Task<bool> PublishAsync(string address, byte[] data)
    {
        // TODO: Implement publishing to OPC UA server
        return false;
    }

    public void Dispose()
    {
        // TODO: Implement disposal of resources
    }

    /// <summary>
    /// Reads data from a specific address in the OPC UA server.
    /// </summary>
    public byte[] ReadData(string address)
    {
        return ReadDataAsync(address).Result;
    }

    /// <summary>
    /// Asynchronously reads data from a specific address in the OPC UA server.
    /// </summary>
    public async Task<byte[]> ReadDataAsync(string address)
    {
        if (_session == null || !IsConnected)
            throw new Exception("Not connected to the server.");

        try
        {
            // Read the node
            var nodeId = new NodeId(address);
            var nodesToRead = new ReadValueIdCollection
        {
            new ReadValueId
            {
                NodeId = nodeId,
                AttributeId = Attributes.Value
            }
        };

            // Create a cancellation token
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken ct = source.Token;

            // Call read service
            var response = await _session.ReadAsync(null, 0, TimestampsToReturn.Both, nodesToRead, ct);

            if (response?.Results == null || response.Results.Count != 1)
                throw new Exception($"Could not read data from address: {address}");

            var result = response.Results[0];

            if (StatusCode.IsBad(result.StatusCode))
                throw new Exception($"Error in reading data from address: {address} with status code: {result.StatusCode}");

            // Assuming the value is a byte array
            var value = result.Value as byte[];
            if (value == null)
                throw new Exception($"Data at address: {address} is not a byte array.");

            return value;
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));
            throw;
        }
    }
    #endregion

    #region Private Methods
    private void Client_KeepAlive(ISession session, KeepAliveEventArgs e)
    {
        if (e.Status != null && ServiceResult.IsNotGood(e.Status))
        {
            //session.DumpToConsole();
            //e.DumpToConsole();
            Console.WriteLine($"{e.Status} : {session.OutstandingRequestCount}/{session.DefunctRequestCount}");

            var reconnectHandler = _reconnectHandler;
            if (reconnectHandler != null) return;

            Console.WriteLine("Connection Status:{0}", "--- RECONNECTING ---");
            reconnectHandler = new SessionReconnectHandler();
            reconnectHandler.BeginReconnect(session, _reconnectPeriod * 1000, Client_ReconnectComplete);
        }
    }

    private Subscription CreateSubscription(Session session)
    {
        Subscription subscription = new Subscription(session.DefaultSubscription)
        { PublishingInterval = _publishingInterval };

        return subscription;
    }

    private void OnMonitoredItemNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
    {
        Console.WriteLine("OPC-UA Received Application Message.");

        try
        {
            var dataValues = monitoredItem.DequeueValues();
            var dataValue = dataValues[0];
            if (dataValue?.Value == null)
                return;

            // Create two different encodings.
            Encoding ascii = Encoding.ASCII;
            Encoding unicode = Encoding.Unicode;

            // Convert unicode string into a byte array.    
            byte[] bytesInUni = unicode.GetBytes(dataValue.Value.ToString() ?? string.Empty);

            // Convert unicode to ascii    
            byte[] bytesInAscii = Encoding.Convert(unicode, ascii, bytesInUni);

            DataReceived?.Invoke(this, new EvtHorizonDataReceivedEventArgs(monitoredItem.DisplayName, bytesInAscii));
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));   // Trigger error event if an exception occurs
        }
    }

    private void Client_ReconnectComplete(object sender, EventArgs e)
    {
        var reconnectHandler = _reconnectHandler;

        // ignore callbacks from discarded objects.
        if (!Object.ReferenceEquals(sender, reconnectHandler))
        {
            return;
        }

        reconnectHandler.Dispose();

        Console.WriteLine("Connection Status: --- RECONNECTED ---");
    }

    private void CertificateValidator_CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
    {
        if (e.Error.StatusCode != StatusCodes.BadCertificateUntrusted) return;

        e.Accept = _autoAccept;
        Console.WriteLine(_autoAccept ? $"Accepted Certificate:{e.Certificate.Subject}" : $"Rejected Certificate:{e.Certificate.Subject}");
    }

    public List<string> Browse()
    {
        return BrowseAsync().Result;
    }

    public async Task<List<string>> BrowseAsync()
    {
        if (_session == null || !IsConnected)
            throw new Exception("Not connected to the server.");

        var nodeIds = new List<string>();

        try
        {
            // Browse from the ObjectsFolder
            await BrowseChildrenAsync(Objects.ObjectsFolder, nodeIds);
            return nodeIds;
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new EvtHorizonErrorEventArgs(ex.Message));
            throw;
        }
    }

    private async Task BrowseChildrenAsync(NodeId nodeId, List<string> nodeIds)
    {
        var nodeToBrowse = new BrowseDescription
        {
            NodeId = nodeId,
            BrowseDirection = BrowseDirection.Forward,
            ReferenceTypeId = ReferenceTypes.HierarchicalReferences,
            NodeClassMask = (uint)NodeClass.Variable | (uint)NodeClass.Object,
            ResultMask = (uint)BrowseResultMask.All
        };

        var nodesToBrowse = new BrowseDescriptionCollection { nodeToBrowse };

        // Create a cancellation token
        CancellationTokenSource source = new CancellationTokenSource();
        CancellationToken ct = source.Token;

        // Call browse service
        var response = await _session.BrowseAsync(null, null, 100, nodesToBrowse, ct);

        if (response?.Results == null)
            throw new Exception("Could not browse the server.");

        foreach (var result in response.Results)
        {
            if (StatusCode.IsBad(result.StatusCode))
                throw new Exception($"Error in browsing with status code: {result.StatusCode}");

            foreach (var reference in result.References)
            {
                var nodeIdAsString = reference.NodeId.ToString();
                nodeIds.Add(nodeIdAsString);
                var childNodeId = NodeId.Parse(nodeIdAsString);
                await BrowseChildrenAsync(childNodeId, nodeIds); // recursively browse children
            }
        }
    }

    /// <summary>
    /// Browses the address space and returns the references found.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="nodeToBrowse">The NodeId for the starting node.</param>
    /// <param name="throwOnError">if set to <c>true</c> a exception will be thrown on an error.</param>
    /// <returns>
    /// The references found. Null if an error occurred.
    /// </returns>
    private static ReferenceDescriptionCollection BrowseTags(Session session, BrowseDescription nodeToBrowse, bool throwOnError)
    {
        try
        {
            ReferenceDescriptionCollection references = new ReferenceDescriptionCollection();

            // construct browse request.
            BrowseDescriptionCollection nodesToBrowse = new BrowseDescriptionCollection();
            nodesToBrowse.Add(nodeToBrowse);

            // start the browse operation.
            BrowseResultCollection results = null;
            DiagnosticInfoCollection diagnosticInfos = null;

            session.Browse(
                null,
                null,
                0,
                nodesToBrowse,
                out results,
                out diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToBrowse);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToBrowse);

            do
            {
                // check for error.
                if (StatusCode.IsBad(results[0].StatusCode))
                {
                    throw new ServiceResultException(results[0].StatusCode);
                }

                // process results.
                for (int ii = 0; ii < results[0].References.Count; ii++)
                {
                    references.Add(results[0].References[ii]);
                }

                // check if all references have been fetched.
                if (results[0].References.Count == 0 || results[0].ContinuationPoint == null)
                {
                    break;
                }

                // continue browse operation.
                ByteStringCollection continuationPoints = new ByteStringCollection();
                continuationPoints.Add(results[0].ContinuationPoint);

                session.BrowseNext(
                    null,
                    false,
                    continuationPoints,
                    out results,
                    out diagnosticInfos);

                ClientBase.ValidateResponse(results, continuationPoints);
                ClientBase.ValidateDiagnosticInfos(diagnosticInfos, continuationPoints);
            }
            while (true);

            //return complete list.
            return references;
        }
        catch (Exception exception)
        {
            if (throwOnError)
            {
                throw new ServiceResultException(exception, StatusCodes.BadUnexpectedError);
            }

            return null;
        }
    }

    /// <summary>
    /// Browses the address space and returns the references found.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="nodesToBrowse">The set of browse operations to perform.</param>
    /// <param name="throwOnError">if set to <c>true</c> a exception will be thrown on an error.</param>
    /// <returns>
    /// The references found. Null if an error occurred.
    /// </returns>
    private static ReferenceDescriptionCollection BrowseTags(Session session, BrowseDescriptionCollection nodesToBrowse, bool throwOnError)
    {
        try
        {
            ReferenceDescriptionCollection references = new ReferenceDescriptionCollection();
            BrowseDescriptionCollection unprocessedOperations = new BrowseDescriptionCollection();

            while (nodesToBrowse.Count > 0)
            {
                // start the browse operation.
                BrowseResultCollection results = null;
                DiagnosticInfoCollection diagnosticInfos = null;

                session.Browse(
                    null,
                    null,
                    0,
                    nodesToBrowse,
                    out results,
                    out diagnosticInfos);

                ClientBase.ValidateResponse(results, nodesToBrowse);
                ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToBrowse);

                ByteStringCollection continuationPoints = new ByteStringCollection();

                for (int ii = 0; ii < nodesToBrowse.Count; ii++)
                {
                    // check for error.
                    if (StatusCode.IsBad(results[ii].StatusCode))
                    {
                        // this error indicates that the server does not have enough simultaneously active 
                        // continuation points. This request will need to be resent after the other operations
                        // have been completed and their continuation points released.
                        if (results[ii].StatusCode == StatusCodes.BadNoContinuationPoints)
                        {
                            unprocessedOperations.Add(nodesToBrowse[ii]);
                        }

                        continue;
                    }

                    // check if all references have been fetched.
                    if (results[ii].References.Count == 0)
                    {
                        continue;
                    }

                    // save results.
                    references.AddRange(results[ii].References);

                    // check for continuation point.
                    if (results[ii].ContinuationPoint != null)
                    {
                        continuationPoints.Add(results[ii].ContinuationPoint);
                    }
                }

                // process continuation points.
                ByteStringCollection revisedContinuationPoints = new ByteStringCollection();

                while (continuationPoints.Count > 0)
                {
                    // continue browse operation.
                    session.BrowseNext(
                        null,
                        false,
                        continuationPoints,
                        out results,
                        out diagnosticInfos);

                    ClientBase.ValidateResponse(results, continuationPoints);
                    ClientBase.ValidateDiagnosticInfos(diagnosticInfos, continuationPoints);

                    for (int ii = 0; ii < continuationPoints.Count; ii++)
                    {
                        // check for error.
                        if (StatusCode.IsBad(results[ii].StatusCode))
                        {
                            continue;
                        }

                        // check if all references have been fetched.
                        if (results[ii].References.Count == 0)
                        {
                            continue;
                        }

                        // save results.
                        references.AddRange(results[ii].References);

                        // check for continuation point.
                        if (results[ii].ContinuationPoint != null)
                        {
                            revisedContinuationPoints.Add(results[ii].ContinuationPoint);
                        }
                    }

                    // check if browsing must continue;
                    continuationPoints = revisedContinuationPoints;
                }

                // check if unprocessed results exist.
                nodesToBrowse = unprocessedOperations;
            }

            // return complete list.
            return references;
        }
        catch (Exception exception)
        {
            if (throwOnError)
            {
                throw new ServiceResultException(exception, StatusCodes.BadUnexpectedError);
            }

            return null;
        }
    }
}
#endregion