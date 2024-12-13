using Microsoft.EntityFrameworkCore;

using HF.EventHorizon.Core.Entities;

namespace HF.EventHorizon.Infrastructure.Data;

public class EvtHorizonContext : DbContext
{
    public DbSet<ProtocolConnection> ProtocolConnections { get; set; }
    public DbSet<DestinationMap> DestinationMaps { get; set; }
    public DbSet<RoutingRule> RoutingRules { get; set; }
    public DbSet<ProtocolPlugin> ProtocolPlugins { get; set; }
    public DbSet<BrowsedAddress> BrowsedAddresses { get; set; }

    public EvtHorizonContext(DbContextOptions<EvtHorizonContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProtocolConnection>()
            .HasIndex(p => p.Name)
            .IsUnique();

        modelBuilder.Entity<DestinationMap>()
            .HasOne(d => d.RoutingRule)
            .WithMany(r => r.DestinationMaps)
            .HasForeignKey(d => d.RoutingRuleId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}
