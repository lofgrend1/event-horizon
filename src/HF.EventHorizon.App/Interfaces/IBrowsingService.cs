using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HF.EventHorizon.App.Interfaces;

/// <summary>
/// Defines the contract for a service that manages BrowsedAddress entities.
/// </summary>
public interface IBrowsingService
{
    /// <summary>
    /// Retrieves all BrowsedAddress entities.
    /// </summary>
    Task<IEnumerable<object>> GetAllBrowsedAddressesAsync();

    /// <summary>
    /// Retrieves a BrowsedAddress entity by its identifier.
    /// </summary>
    Task<object> GetBrowsedAddressByIdAsync(int id);

    /// <summary>
    /// Adds a new BrowsedAddress entity.
    /// </summary>
    Task<object> AddBrowsedAddressAsync(object browsedAddress);

    /// <summary>
    /// Updates an existing BrowsedAddress entity.
    /// </summary>
    Task UpdateBrowsedAddressAsync(object browsedAddress);

    /// <summary>
    /// Removes a BrowsedAddress entity by its identifier.
    /// </summary>
    Task DeleteBrowsedAddressAsync(int id);

    Task<IEnumerable<object>> GetAllBrowsedAddressesForConnectionAsync(int connectionId);

    void RefreshAddress(int id, List<string> browsedTags);

    Task RefreshAddressAsync(int id, List<string> browsedTags);
}