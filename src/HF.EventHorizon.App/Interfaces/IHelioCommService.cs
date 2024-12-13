namespace HF.EventHorizon.App.Interfaces;

public interface IHelioCommService
{
    IDestinationMapService DestinationMapService { get; }
    IProtocolConnectionService ProtocolConnectionService { get; }
    IProtocolPluginService ProtocolPluginService { get; }
    IRoutingRuleService RoutingRuleService { get; }
    IBrowsingService BrowsingService { get; }
}