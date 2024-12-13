using HF.EventHorizon.Core.Entities;

namespace HF.EventHorizon.App.Interfaces;

public interface IProtocolConnectionService
{
    Task<IEnumerable<ProtocolConnection>> GetAllProtocolConnectionsAsync();
    Task<ProtocolConnection> GetProtocolConnectionByIdAsync(int id);
    Task<ProtocolConnection> AddProtocolConnectionAsync(ProtocolConnection protocolConnection);
    Task UpdateProtocolConnectionAsync(ProtocolConnection protocolConnection);
    Task DeleteProtocolConnectionAsync(int id);
}