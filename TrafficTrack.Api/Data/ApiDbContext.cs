using Microsoft.EntityFrameworkCore;

namespace TrafficTrack.Api.Data;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<SavedAreaEntity> SavedAreas { get; set; }
    public DbSet<TrafficFlowEntity> TrafficFlows { get; set; }
    public DbSet<TrafficEventEntity> TrafficEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserEntity>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Username).IsUnique();
            e.HasMany(u => u.SavedAreas).WithOne(a => a.User).HasForeignKey(a => a.UserId);
        });

        modelBuilder.Entity<SavedAreaEntity>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.UserId);
        });

        modelBuilder.Entity<TrafficFlowEntity>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.RecordedAt);
            e.HasIndex(t => new { t.Latitude, t.Longitude });
        });

        modelBuilder.Entity<TrafficEventEntity>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.EventId);
            e.HasIndex(t => t.Type);
            e.HasIndex(t => t.RecordedAt);
            e.HasIndex(t => new { t.Latitude, t.Longitude });
        });
    }
}

public class UserEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public ICollection<SavedAreaEntity> SavedAreas { get; set; } = [];
}

public class SavedAreaEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Lat1 { get; set; }
    public double Lon1 { get; set; }
    public double Lat2 { get; set; }
    public double Lon2 { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
    public UserEntity User { get; set; } = null!;
}

public class TrafficFlowEntity
{
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string RoadName { get; set; } = string.Empty;
    public double CurrentSpeed { get; set; }
    public double FreeFlowSpeed { get; set; }
    public double CurrentTravelTime { get; set; }
    public double FreeFlowTravelTime { get; set; }
    public double Confidence { get; set; }
    public DateTime RecordedAt { get; set; }
}

public class TrafficEventEntity
{
    public int Id { get; set; }
    public string EventId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string RoadName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime RecordedAt { get; set; }
}
