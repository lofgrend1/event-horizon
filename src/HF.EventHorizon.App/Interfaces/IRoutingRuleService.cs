namespace HF.EventHorizon.App.Interfaces;

public interface IRoutingRuleService
{
    Task<IEnumerable<object>> GetAllRoutingRulesAsync();
    Task<object> GetRoutingRuleByIdAsync(int id);
    Task<object> AddRoutingRuleAsync(object routingRule);
    Task UpdateRoutingRuleAsync(object routingRule);
    Task DeleteRoutingRuleAsync(int id);
}