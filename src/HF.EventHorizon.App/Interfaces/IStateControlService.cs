using HF.EventHorizon.App.Models;

namespace HF.EventHorizon.App.Interfaces;

public interface IStateControlService
{
    event EventHandler<StateChangeRequestedEventArgs> StateChangeRequested;
    void Restart();
    void Stop();
    void Start();
}
