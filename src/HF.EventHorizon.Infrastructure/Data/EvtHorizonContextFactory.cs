using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HF.EventHorizon.Infrastructure.Data;

public class EvtHorizonContextFactory : IDesignTimeDbContextFactory<EvtHorizonContext>
{
    public EvtHorizonContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EvtHorizonContext>();

        // Build configuration
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("HF.EventHorizon.Infrastructure"));

        return new EvtHorizonContext(optionsBuilder.Options);
    }
}
