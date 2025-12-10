namespace TrafficTrack.Core.Models;

public class TrafficIncident
{
    public int Id { get; set; }
    public string IncidentId { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime RecordedAt { get; set; }
    public string RoadName { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public int? MonitoringSessionId { get; set; }
}
