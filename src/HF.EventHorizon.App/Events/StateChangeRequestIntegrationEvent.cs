using HF.EventBus.Events;
using HF.EventHorizon.Core.Enums;

namespace HF.EventHorizon.App.Events;

public record StateChangeRequestIntegrationEvent : IntegrationEvent
{
    public StateCommand RequestedState { get; set; }
}