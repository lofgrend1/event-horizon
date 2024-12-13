using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace HF.EventHorizon.Core.Entities;

/// <summary>
/// Represents a connection that uses a specific protocol plugin.
/// </summary>
public class ProtocolConnection : BaseEntity
{
    /// <summary>
    /// The Unique Name for the connection to a protocol
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// The ID of the associated protocol plugin.
    /// </summary>
    public int ProtocolPluginId { get; set; }

    /// <summary>
    /// The associated protocol plugin.
    /// </summary>
    public ProtocolPlugin? ProtocolPlugin { get; set; }

    /// <summary>
    /// The connection string.
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// Additional parameters for the connection represented as a JSON string.
    /// </summary>
    public string AdditionalParametersJson { get; set; } = "{}";

    /// <summary>
    /// The routing rules for this protocol connection.
    /// </summary>
    public virtual ICollection<RoutingRule>? RoutingRules { get; set; }

    /// <summary>
    /// Additional parameters for the connection represented as a dictionary.
    /// </summary>
    [NotMapped]
    public Dictionary<string, string> AdditionalParameters
    {
        get => JsonConvert.DeserializeObject<Dictionary<string, string>>(AdditionalParametersJson) ?? [];
        set => AdditionalParametersJson = JsonConvert.SerializeObject(value);
    }

    public void AddParameter(string key, string value)
    {
        if (!this.AdditionalParameters.ContainsKey(key))
        {
            var parameters = this.AdditionalParameters;
            parameters.Add(key, value);
            this.AdditionalParameters = parameters;
        }
        else
        {
            this.AdditionalParameters[key] = value;
        }
    }

    public void RemoveParameter(string key)
    {
        if (this.AdditionalParameters.ContainsKey(key))
        {
            var parameters = this.AdditionalParameters;
            parameters.Remove(key);
            this.AdditionalParameters = parameters;
        }
    }

    public void UpdateParameter(string key, string value)
    {
        if (this.AdditionalParameters.ContainsKey(key))
        {
            var parameters = this.AdditionalParameters;
            parameters[key] = value;
            this.AdditionalParameters = parameters;
        }
    }
}