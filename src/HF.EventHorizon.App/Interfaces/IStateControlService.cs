using HF.EventHorizon.Core.Events;

namespace HF.EventHorizon.App.Interfaces;

public interface IStateControlService
{
    event EventHandler<StateChangeRequestedEventArgs> StateChangeRequested;
    void Restart();
    void Stop();
    void Start();
}
