namespace TrafficTrack.Core.Models;

public class TrafficFlow
{
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double CurrentSpeed { get; set; }
    public double FreeFlowSpeed { get; set; }
    public double CurrentTravelTime { get; set; }
    public double FreeFlowTravelTime { get; set; }
    public double Confidence { get; set; }
    public string RoadName { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public string Area { get; set; } = string.Empty;
    public int? MonitoringSessionId { get; set; }
}
