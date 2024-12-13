namespace HF.EventBus.Abstractions;

public abstract class SerializedMessage
{
    public Guid UUID { get; }
    public string InstanceId { get; }
    public DateTime CreationDate { get; }

    protected SerializedMessage(string instanceId)
    {
        CreationDate = DateTime.Now;
        UUID = Guid.NewGuid();
        InstanceId = instanceId ?? UUID.ToString();
    }
}