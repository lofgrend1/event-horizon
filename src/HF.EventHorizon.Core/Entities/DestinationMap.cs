using System.ComponentModel.DataAnnotations.Schema;

using HF.EventHorizon.Core.Interfaces;

namespace HF.EventHorizon.Core.Entities;

/// <summary>
/// Represents a mapping between an address and a protocol connection.
/// </summary>
public class DestinationMap : BaseEntity
{
    /// <summary>
    /// The ID of the associated protocol connection.
    /// </summary>
    public int ProtocolConnectionId { get; set; }

    /// <summary>
    /// The associated protocol connection.
    /// </summary>
    public ProtocolConnection ProtocolConnection { get; set; }

    public int RoutingRuleId { get; set; }
    public RoutingRule RoutingRule { get; set; }

    /// <summary>
    /// The destination address.
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// Gets the IDestination instance representing the destination of the map.
    /// </summary>
    [NotMapped]
    public IDestination? Destination { get; set; }

    /// <summary>
    /// Initializes a new instance of the DestinationMap class with the provided protocol id and address.
    /// </summary>
    /// <param name="protocolConnectionId"></param>
    /// <param name="address">A string representing the address to be mapped with the destination.</param>
    public DestinationMap(int protocolConnectionId, string address)
    {
        ProtocolConnectionId = protocolConnectionId;
        Address = address;
        ProtocolConnection = new ProtocolConnection(); // Initialize with a default value
        RoutingRule = new RoutingRule(); // Initialize with a default value
    }

    /// <summary>
    /// Initializes a new instance of the DestinationMap class with the provided destination and address.
    /// </summary>
    /// <param name="destination">An IDestination instance representing the destination to be mapped.</param>
    /// <param name="address">A string representing the address to be mapped with the destination.</param>
    public DestinationMap(IDestination destination, string address)
    {
        Destination = destination;
        Address = address;
        ProtocolConnection = new ProtocolConnection(); // Initialize with a default value
        RoutingRule = new RoutingRule(); // Initialize with a default value
    }
}