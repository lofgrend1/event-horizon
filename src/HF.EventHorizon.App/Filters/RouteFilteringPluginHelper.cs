using System.Reflection;

using HF.EventHorizon.App.Interfaces;

namespace HF.EventHorizon.App.Filters;

public class RouteFilteringPluginHelper
{
    public static List<IRoutingFilterPlugin> LoadPlugins(string directoryPath)
    {
        // Get the DLL files in the specified directory
        string[] pluginFiles = Directory.GetFiles(directoryPath, "*.dll");
        var plugins = new List<IRoutingFilterPlugin>();

        foreach (string pluginFile in pluginFiles)
        {
            try
            {
                // Load the plugin assembly
                Assembly pluginAssembly = Assembly.LoadFrom(pluginFile);

                // Get the types in the assembly that implement the IProtocolPlugin interface
                IEnumerable<Type> pluginTypes = pluginAssembly.GetTypes()
                    .Where(t => typeof(IRoutingFilterPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                // Create an instance of each plugin type and add it to the list
                foreach (Type pluginType in pluginTypes)
                {
                    var pluginInstance = Activator.CreateInstance(pluginType);
                    if (pluginInstance is IRoutingFilterPlugin plugin)
                    {
                        plugins.Add(plugin);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Error creating instance of type '{pluginType.FullName}' from file '{pluginFile}'.");
                    }
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