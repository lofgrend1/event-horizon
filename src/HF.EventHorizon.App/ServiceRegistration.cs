using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using HF.EventHorizon.App.Settings;

namespace HF.EventHorizon.App;

public static class ServiceRegistration
{
    public static void AddClientSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var settingsConfig = configuration.GetSection("RouteMgrSettings");
        services.Configure<RouteMgrSettings>(settingsConfig);
        services.AddOptions();
    }

    public static void AddServerSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServerSettings>(configuration.GetSection("ServerSettings"));
    }

    public static void AddServerEventHandlers(this IServiceCollection services, IConfiguration configuration)
    {
        // Add integration event handlers.
        //services.AddTransient<IIntegrationEventHandler<ConnectionStateChangedIntegrationEvent>, ConnectionStateChangedIntegrationEventHandler>();
        //services.AddTransient<IIntegrationEventHandler<DataRecievedIntegrationEvent>, DataRecievedIntegrationEventHandler>();
        //services.AddTransient<IIntegrationEventHandler<DataRoutedIntegrationEvent>, DataRoutedIntegrationEventHandler>();
        //services.AddTransient<IIntegrationEventHandler<PluginErrorIntegrationEvent>, PluginErrorIntegrationEventHandler>();
        //services.AddTransient<IIntegrationEventHandler<StateChangeRequestIntegrationEvent>, StateChangeRequestIntegrationEventHandler>();
    }

    public static void AddEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCap(options =>
        {
            options.UseInMemoryStorage();
            options.UseDashboard();

            options.UseRabbitMQ(conf =>
            {
                conf.HostName = configuration["EventBus:HostName"] ?? string.Empty;
                conf.Port = int.Parse(configuration["EventBus:Port"] ?? string.Empty);
                conf.UserName = configuration["EventBus:UserName"] ?? string.Empty;
                conf.Password = configuration["EventBus:Password"] ?? string.Empty;
            });

            options.DefaultGroupName = configuration["EventBus:DefaultGroupName"] ?? "HF.EventHorizon.App";
            options.FailedRetryCount = 5;
            options.FailedRetryInterval = 5;
        });
    }
}
