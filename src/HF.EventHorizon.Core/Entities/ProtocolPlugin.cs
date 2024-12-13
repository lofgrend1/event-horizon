using System.ComponentModel.DataAnnotations.Schema;

namespace HF.EventHorizon.Core.Entities;

/// <summary>
/// Represents a plugin for a specific protocol.
/// </summary>
public class ProtocolPlugin : BaseEntity
{
    /// <summary>
    /// The name of the plugin.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The version of the plugin.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// The path to the directory containing the plugin.
    /// </summary>
    public string PluginDirectoryPath { get; set; } = string.Empty;

    public string PluginTypesCsv { get; set; } = string.Empty;

    [NotMapped]
    public string[] PluginTypes
    {
        get => PluginTypesCsv.Split(new[] { '\u002C' }, StringSplitOptions.RemoveEmptyEntries);
        set => PluginTypesCsv = string.Join("\u002C", value);
    }

    public void AddType(string type)
    {
        var types = PluginTypes.ToList();
        if (!types.Contains(type))
        {
            types.Add(type);
            PluginTypes = types.ToArray();
        }
    }

    public void RemoveType(string type)
    {
        var types = PluginTypes.ToList();
        if (types.Contains(type))
        {
            types.Remove(type);
            PluginTypes = types.ToArray();
        }
    }

    public string RequiredParametersCsv { get; set; } = string.Empty;

    [NotMapped]
    public string[] RequiredParameters
    {
        get => RequiredParametersCsv.Split(new[] { '\u002C' }, StringSplitOptions.RemoveEmptyEntries);
        set => RequiredParametersCsv = string.Join("\u002C", value);
    }

    public void AddRequiredParameter(string parameter)
    {
        var parameters = RequiredParameters.ToList();
        if (!parameters.Contains(parameter))
        {
            parameters.Add(parameter);
            RequiredParameters = [.. parameters];
        }
    }

    public void RemoveRequiredParameter(string parameter)
    {
        var parameters = RequiredParameters.ToList();
        if (parameters.Contains(parameter))
        {
            parameters.Remove(parameter);
            RequiredParameters = [.. parameters];
        }
    }
}