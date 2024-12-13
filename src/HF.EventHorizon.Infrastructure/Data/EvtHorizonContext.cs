using Microsoft.EntityFrameworkCore;

namespace HF.EventHorizon.Infrastructure.Data;

public class EvtHorizonContext : DbContext
{
    public EvtHorizonContext(DbContextOptions<EvtHorizonContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
