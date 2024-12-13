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
        var dbType = configuration.GetSection("DatabaseType").Value; // Fix for CS1061

        switch (dbType)
        {
            case "MySQL":
                optionsBuilder.UseMySQL(connectionString ?? throw new ArgumentNullException(nameof(connectionString)), b => b.MigrationsAssembly("HF.EventHorizon.Infrastructure")); // Fix for CS8604
                break;
            case "PostgreSQL":
                optionsBuilder.UseNpgsql(connectionString ?? throw new ArgumentNullException(nameof(connectionString)), b => b.MigrationsAssembly("HF.EventHorizon.Infrastructure")); // Fix for CS8604
                break;
            default:
                optionsBuilder.UseSqlServer(connectionString ?? throw new ArgumentNullException(nameof(connectionString)), b => b.MigrationsAssembly("HF.EventHorizon.Infrastructure")); // Fix for CS8604
                break;
        }

        return new EvtHorizonContext(optionsBuilder.Options);
    }
}
