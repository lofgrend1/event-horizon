using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace HF.EventHorizon.Agent;

internal class Program
{
    public static readonly string AppName = typeof(Program).Assembly.GetName().Name ?? "UnknownAppName";
    public static readonly string AppBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

    public static void Main(string[] args)
    {
        JsonConvert.DefaultSettings = () =>
            new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        CreateHostBuilder(args).Run();
    }

    public static IHost CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((host, builder) =>
            {
                builder.SetBasePath(Directory.GetCurrentDirectory());
                builder.AddJsonFile(AppBaseDirectory + "appsettings.json", optional: true);
                builder.AddJsonFile(
                    AppBaseDirectory + $"appsettings.{host.HostingEnvironment.EnvironmentName}.json",
                    optional: true);
                builder.AddEnvironmentVariables();
                builder.AddCommandLine(args);
            })
            .ConfigureServices((hostContext, services) =>
            {
                //services.AddClientSettings(hostContext.Configuration);
                //services.AddEventBus(hostContext.Configuration);

                //services.AddTransient<IPrintJobRequestedHandler, PrintJobRequestedHandler>();
                //services.AddSingleton<IPrinterService, PrinterService>();
            })
            .Build();
}
