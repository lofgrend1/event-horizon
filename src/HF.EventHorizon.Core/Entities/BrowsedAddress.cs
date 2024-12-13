namespace HF.EventHorizon.Core.Entities;

public class BrowsedAddress : BaseEntity
{
    public int ProtocolConnectionId { get; set; }
    public ProtocolConnection? ProtocolConnection { get; set; }
    public string Address { get; set; } = null!;
}