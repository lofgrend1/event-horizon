namespace HF.EventHorizon.Core.Entities;

/// <summary>
/// Represents a routing rule, including associated protocol connection and destination maps.
/// </summary>
public class RoutingRule : BaseEntity
{
    /// <summary>
    /// The ID of the associated protocol connection.
    /// </summary>
    public int ProtocolConnectionId { get; set; }

    /// <summary>
    /// The associated protocol connection.
    /// </summary>
    public ProtocolConnection? ProtocolConnection { get; set; }

    /// <summary>
    /// The address for this routing rule.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// The destination maps associated with this routing rule.
    /// </summary>
    public virtual ICollection<DestinationMap>? DestinationMaps { get; set; }
}