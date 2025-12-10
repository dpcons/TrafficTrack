namespace TrafficTrack.Core.Models;

public class MonitoringSession
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int IntervalSeconds { get; set; }
    public double Latitude1 { get; set; }
    public double Longitude1 { get; set; }
    public double Latitude2 { get; set; }
    public double Longitude2 { get; set; }
    public string Status { get; set; } = "Scheduled"; // Scheduled, Running, Completed, Cancelled, Error
    public DateTime CreatedAt { get; set; }
    public DateTime? LastCollectionAt { get; set; }
    public int CollectionCount { get; set; }
    public string? Notes { get; set; }
    public DateTime? NextRunAt { get; set; } // prossima esecuzione pianificata
    public int ErrorCount { get; set; } // errori consecutivi raccolta
    public string? LastErrorMessage { get; set; }

    public BoundingBox GetBoundingBox() => new BoundingBox(Latitude1, Longitude1, Latitude2, Longitude2);
}
