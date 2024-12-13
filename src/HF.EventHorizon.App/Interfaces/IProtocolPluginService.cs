using HF.EventHorizon.Core.Entities;

namespace HF.EventHorizon.App.Interfaces;

public interface IProtocolPluginService
{
    Task<IEnumerable<ProtocolPlugin>> GetAllProtocolPluginsAsync();
    Task<ProtocolPlugin> GetProtocolPluginByIdAsync(int id);
    Task<ProtocolPlugin> AddProtocolPluginAsync(ProtocolPlugin protocolPlugin);
    Task UpdateProtocolPluginAsync(ProtocolPlugin protocolPlugin);
    Task DeleteProtocolPluginAsync(int id);
}
