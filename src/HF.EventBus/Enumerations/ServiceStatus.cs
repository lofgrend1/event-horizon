namespace HF.EventBus.Enumerations;

public enum ServiceStatus
{
    Faulted = -1,
    Starting = 0,
    Started = 1,
    Running = 2,
    Stopping = 3,
    Stopped = 4
}