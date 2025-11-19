using Microsoft.EntityFrameworkCore;
using TrafficTrack.Core.Models;

namespace TrafficTrack.Infrastructure.Data;

public class TrafficDbContext : DbContext
{
    public TrafficDbContext(DbContextOptions<TrafficDbContext> options) : base(options)
    {
    }

    public DbSet<TrafficIncident> TrafficIncidents { get; set; }
    public DbSet<TrafficFlow> TrafficFlows { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TrafficIncident>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RecordedAt);
            entity.HasIndex(e => e.Area);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.IncidentId);
        });

        modelBuilder.Entity<TrafficFlow>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RecordedAt);
            entity.HasIndex(e => e.Area);
        });
    }
}
