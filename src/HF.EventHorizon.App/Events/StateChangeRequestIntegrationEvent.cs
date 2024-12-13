using HF.EventBus.Events;
using HF.EventHorizon.App.Models;

namespace HF.EventHorizon.App.Events;

public record StateChangeRequestIntegrationEvent : IntegrationEvent
{
    public StateCommand RequestedState { get; set; }
}