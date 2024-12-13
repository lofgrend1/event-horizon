namespace HF.EventHorizon.App.Models;

public class StateChangeRequestedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the timestamp when the status change occurred.
    /// </summary>
    public DateTime TimeStamp { get; }

    public StateCommand StateCommand { get; }

    /// <summary>
    /// Initializes a new instance of the ConnectionStateChangedEventArgs class.
    /// </summary>
    /// <param name="status">The new connection status.</param>
    /// <param name="message">An optional message associated with the status change.</param>
    public StateChangeRequestedEventArgs(StateCommand stateCommand)
    {
        TimeStamp = DateTime.UtcNow;
        StateCommand = stateCommand;
    }
}
