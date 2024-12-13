using System.Reflection;

using HF.EventHorizon.Core.Interfaces;

namespace HF.EventHorizon.App.Models;

public static class ProtocolPluginHelper
{
    public static List<IProtocolPlugin> LoadPlugins(string directoryPath)
    {
        // Get the DLL files in the specified directory
        string[] pluginFiles = Directory.GetFiles(directoryPath, "*.dll");
        var plugins = new List<IProtocolPlugin>();

        foreach (string pluginFile in pluginFiles)
        {
            try
            {
                // Load the plugin assembly
                Assembly pluginAssembly = Assembly.LoadFrom(pluginFile);

                // Get the types in the assembly that implement the IProtocolPlugin interface
                IEnumerable<Type> pluginTypes = pluginAssembly.GetTypes()
                    .Where(t => typeof(IProtocolPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                // Create an instance of each plugin type and add it to the list
                foreach (Type pluginType in pluginTypes)
                {
                    var plugin = Activator.CreateInstance(pluginType) as IProtocolPlugin ?? throw new InvalidOperationException($"Failed to create an instance of {pluginType.FullName}");
                    plugins.Add(plugin);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading plugin from file '{pluginFile}': {ex.Message}");
                throw;
            }
        }
        return plugins;
    }
}
