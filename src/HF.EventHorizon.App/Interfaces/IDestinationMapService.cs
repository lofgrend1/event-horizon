using HF.EventHorizon.Core.Entities;

namespace HF.EventHorizon.App.Interfaces;

/// <summary>
/// Defines the contract for a service that manages DestinationMap entities.
/// </summary>
public interface IDestinationMapService
{
    /// <summary>
    /// Retrieves all DestinationMap entities.
    /// </summary>
    Task<IEnumerable<DestinationMap>> GetAllDestinationMapsAsync();

    /// <summary>
    /// Retrieves a DestinationMap entity by its identifier.
    /// </summary>
    Task<DestinationMap> GetDestinationMapByIdAsync(int id);

    /// <summary>
    /// Adds a new DestinationMap entity.
    /// </summary>
    Task<DestinationMap> AddDestinationMapAsync(DestinationMap destinationMap);

    /// <summary>
    /// Updates an existing DestinationMap entity.
    /// </summary>
    Task UpdateDestinationMapAsync(DestinationMap destinationMap);

    /// <summary>
    /// Removes a DestinationMap entity by its identifier.
    /// </summary>
    Task DeleteDestinationMapAsync(int id);

    Task<IEnumerable<DestinationMap>> GetAllDestinationMapsForRuleAsync(int ruleId);
}