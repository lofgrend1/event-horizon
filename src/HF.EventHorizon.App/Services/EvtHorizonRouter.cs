using Microsoft.Extensions.Logging;
using HF.EventHorizon.App.Models;
using HF.EventHorizon.Core.Interfaces;
using HF.EventHorizon.Core.Entities;

namespace HF.EventHorizon.App.Services;

/// <summary>
/// Manages the routing of data based on defined rules. 
/// The routing rules are defined based on a plugin ID and an address.
/// </summary>
public class EvtHorizonRouter
{
    private readonly ILogger _logger;
    private Dictionary<(string PluginId, string Address), List<DestinationMap>> _routingRules;

    /// <summary>
    /// Initializes a new instance of the Router class.
    /// </summary>
    public EvtHorizonRouter(ILogger logger)
    {
        _logger = logger;
        _routingRules = new Dictionary<(string PluginId, string Address), List<DestinationMap>>();
    }

    /// <summary>
    /// Defines a new routing rule.
    /// </summary>
    /// <param name="pluginId">The ID of the plugin.</param>
    /// <param name="address">The address related to the plugin.</param>
    /// <param name="destinationMap">The destination map for this routing rule.</param>
    public void DefineRoutingRule(string pluginId, string address, List<DestinationMap> destinationMap)
    {
        _logger.LogInformation($"Defining routing rule for pluginId: {pluginId}, address: {address}. Destination maps count: {destinationMap.Count}");

        _routingRules[(pluginId, address)] = destinationMap;
    }

    /// <summary>
    /// Routes the data based on defined rules.
    /// </summary>
    /// <param name="plugin">The plugin where data is received.</param>
    /// <param name="e">The event arguments that include data and address.</param>
    public void RouteData(IProtocolPlugin plugin, EvtHorizonDataReceivedEventArgs e)
    {
        var data = e.Data;
        var address = e.Address;

        if (address == null)
        {
            _logger.LogWarning($"Address is null for pluginId: {plugin.PluginId}");
            return;
        }

        var routingKey = (plugin.PluginId, Address: address);

        try
        {
            if (_routingRules.ContainsKey(routingKey))
            {
                _logger.LogInformation($"Routing data for pluginId: {plugin.PluginId}, address: {address}. Destination maps count: {_routingRules[routingKey].Count}");

                foreach (var route in _routingRules[routingKey])
                {
                    route.Destination.Publish(route.Address, data);
                }
            }
            else
            {
                _logger.LogWarning($"No routing rule defined for pluginId: {plugin.PluginId}, address: {address}");
            }
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, $"Routing key not found: {routingKey}");
        }
    }

    /// <summary>
    /// Asynchronously routes the data based on defined rules.
    /// </summary>
    /// <param name="plugin">The plugin where data is received.</param>
    /// <param name="e">The event arguments that include data and address.</param>
    public async Task RouteDataAsync(IProtocolPlugin plugin, EvtHorizonDataReceivedEventArgs e)
    {
        var data = e.Data;
        var address = e.Address;

        _logger.LogDebug($"[DEBUG] Received data: {data}, Address: {address}");

        if (address == null)
        {
            _logger.LogWarning($"Address is null for pluginId: {plugin.PluginId}");
            return;
        }

        var routingKey = (plugin.PluginId, Address: address);

        _logger.LogDebug($"[DEBUG] Formed routingKey: {routingKey}");

        try
        {
            if (_routingRules.ContainsKey(routingKey))
            {
                _logger.LogDebug($"Routing data async for pluginId: {plugin.PluginId}, address: {address}. Destination maps count: {_routingRules[routingKey].Count}");

                if (_routingRules[routingKey].Count == 0)
                {
                    _logger.LogDebug($"[DEBUG] No routes found for the key: {routingKey}");
                }

                foreach (var route in _routingRules[routingKey])
                {
                    _logger.LogDebug($"[DEBUG] Sending data: {data} to destination: {route.Destination} with address: {route.Address}");
                    await route.Destination.PublishAsync(route.Address, data);
                    _logger.LogDebug($"[DEBUG] Data sent successfully to destination: {route.Destination} with address: {route.Address}");
                }
            }
            else
            {
                _logger.LogWarning($"No routing rule defined for pluginId: {plugin.PluginId}, address: {address}");
            }
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, $"Routing key not found: {routingKey}");
        }
    }
}