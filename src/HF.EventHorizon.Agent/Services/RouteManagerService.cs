using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using HF.EventHorizon.App.Interfaces;
using HF.EventHorizon.App.Settings;
using HF.EventHorizon.App.Events;
using HF.EventHorizon.App.Services;
using HF.EventHorizon.Core.Entities;
using HF.EventHorizon.Core.Interfaces;
using HF.EventBus.Abstractions;
using HF.EventHorizon.Core.Enums;
using HF.EventHorizon.App.Helpers;
using HF.EventHorizon.Core.Events;

namespace HF.EventHorizon.Agent.Services;

public class RouteManagerService : BackgroundService
{
    #region Fields

    private readonly ILogger<RouteManagerService> _logger;
    private readonly RouteMgrSettings _settings;
    private readonly IHelioCommService _helioCommService;
    private readonly IStateControlService _stateControlService;
    private readonly IEventBus _eventBus;

    private bool _initialized = false;

    private static List<IProtocolPlugin> _plugins = null!;
    private static EvtHorizonRouter _router = null!;

    #endregion

    public RouteManagerService(IOptions<RouteMgrSettings> settings
        , ILogger<RouteManagerService> logger
        , IHelioCommService helioCommService
        , IStateControlService stateControlService
        , IEventBus eventBus
    )
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _helioCommService = helioCommService ?? throw new ArgumentNullException(nameof(helioCommService));
        _stateControlService = stateControlService ?? throw new ArgumentException(null, nameof(stateControlService));
        _eventBus = eventBus ?? throw new ArgumentException(null, nameof(eventBus));

        _plugins = [];
        _router = new EvtHorizonRouter(_logger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("RouteManagerService is starting.");

        stoppingToken.Register(() => _logger.LogDebug("#1 RouteManagerService background task is stopping."));

        _stateControlService.StateChangeRequested += OnStateChangeRequested;

        await Initialize();
        while (!stoppingToken.IsCancellationRequested)
        {
            // DO WORK
            await Task.Delay(_settings.CheckUpdateTime, stoppingToken);
        }

        _logger.LogDebug("RouteManagerService background task is stopping.");
    }

    #region Private Methods

    private async Task Initialize()
    {
        try
        {
            var plugins = await _helioCommService.ProtocolPluginService.GetAllProtocolPluginsAsync();
            foreach (var plugin in plugins)
                _plugins.AddRange(ProtocolPluginHelper.LoadPlugins(plugin.PluginDirectoryPath));


            var connections = await _helioCommService.ProtocolConnectionService.GetAllProtocolConnectionsAsync();
            foreach (var connection in connections)
            {
                // Debug output
                Console.WriteLine($"Connection: {connection.Id}, ProtocolPlugin Name: {connection.ProtocolPlugin?.Name}");
                foreach (var plugin in _plugins)
                {
                    Console.WriteLine($"Plugin: {plugin.GetType().Name.ToLower().Replace("plugin", "")}");
                }

                var idx = _plugins.Select((item, index) => new { Item = item, Index = index })
                    .FirstOrDefault(x => x.Item.GetType().Name.ToLower().Replace("plugin", "") ==
                        connection.ProtocolPlugin?.Name.Split('.').Last().ToLower().Replace("plugin", ""))?.Index ?? -1;

                Console.WriteLine($"Index: {idx}");  // This will print the index for each connection

                await ConnectToPlugin(connection, connection.Id.ToString(), idx);
                foreach (var rule in connection?.RoutingRules ?? [])
                {
                    var destinationMaps = await _helioCommService.DestinationMapService.GetAllDestinationMapsForRuleAsync(rule.Id);
                    var destinations = new List<DestinationMap>();

                    foreach (var destinationMap in destinationMaps)
                    {
                        var destIdx = _plugins.Select((item, index) => new { Item = item, Index = index })
                            .FirstOrDefault(x => x.Item.GetType().Name.ToLower().Replace("plugin", "") ==
                                destinationMap.ProtocolConnection.ProtocolPlugin?.Name.Split('.').Last().ToLower().Replace("plugin", ""))?.Index ?? -1;

                        var plugin = _plugins[destIdx];
                        if (plugin is IDestination destinationPlugin)
                            destinations.Add(new DestinationMap(destinationPlugin, destinationMap.Address));
                    }

                    if (_plugins[idx] is ISubscribable sourcePlugin)
                        sourcePlugin.Subscribe(rule.Address);

                    _router.DefineRoutingRule(_plugins[idx].PluginId, rule.Address, destinations);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the RouteManagerService.");
            throw;
        }
        _initialized = true;
    }

    private async Task ConnectToPlugin(ProtocolConnection connection, string pluginId, int pluginIndex = 0)
    {
        // Check if the index is valid
        if (pluginIndex < 0 || pluginIndex >= _plugins.Count)
        {
            _logger.LogError("Invalid plugin index: {PluginIndex}", pluginIndex);
            return;
        }

        // If the index is valid, proceed with the rest of the function
        IProtocolPlugin selectedPlugin = _plugins[pluginIndex];

        selectedPlugin.ConnectionStateChanged += (sender, e) => OnConnectionStateChanged(sender, e, _eventBus);
        selectedPlugin.Error += (sender, e) => OnError(sender!, e, _eventBus);
        selectedPlugin.PluginId = pluginId;

        if (selectedPlugin is ISubscribable subscribablePlugin)
            subscribablePlugin.DataReceived += (sender, e) => OnDataReceived(sender, e, _eventBus);

        await selectedPlugin.ConnectAsync(connection.ConnectionString, connection.AdditionalParameters);

        if (selectedPlugin is IBrowsable browsablePlugin)
        {
            var browsedTags = await browsablePlugin.BrowseAsync();
            await _helioCommService.BrowsingService.RefreshAddressAsync(connection.Id, browsedTags);
        }
    }

    private async void OnStateChangeRequested(object? sender, StateChangeRequestedEventArgs e)
    {
        switch (e.StateCommand)
        {
            case StateCommand.Restart:
                await RestartService();
                break;
            case StateCommand.Start:
                await StartService();
                break;
            case StateCommand.Stop:
                await StopService();
                break;
            default:
                break;
        }
    }

    private async Task StartService()
    {
        await Initialize();
    }

    private async Task StopService()
    {
        _initialized = false;

        foreach (var plugin in _plugins)
            await plugin.DisconnectAsync();

        _plugins.Clear();
    }

    private async Task RestartService()
    {
        await StopService();
        await StartService();
    }

    #endregion

    #region Event Handlers

    private void OnError(object sender, EvtHorizonErrorEventArgs e, IEventBus eventBus)
    {
        eventBus.Publish(new PluginErrorIntegrationEvent(e.ErrorMessage, e.Address, e.ErrorCode));
        _logger.LogError($"[{e.ErrorCode}] {e.ErrorMessage} : {e.ErrorTimeStamp} | Address:{e.Address}");
    }

    private async void OnDataReceived(object? sender, EvtHorizonDataReceivedEventArgs e, IEventBus eventBus)
    {
        if (_initialized)
        {
            if (sender is IProtocolPlugin plugin)
            {
                eventBus.Publish(new DataRoutedIntegrationEvent());

                await _router.RouteDataAsync(plugin, e);
            }

            eventBus.Publish(new DataRecievedIntegrationEvent(e.Address, e.Data));
            Console.WriteLine($"[{sender}] Data: {Encoding.UTF8.GetString(e.Data)}");
        }
    }

    private void OnConnectionStateChanged(object? sender, ConnectionStateChangedEventArgs e, IEventBus eventBus)
    {
        eventBus.Publish(new ConnectionStateChangedIntegrationEvent(e.Status, e.Message));
        Console.WriteLine($"Connection State Changed: {e.Status}");
    }

    #endregion
}