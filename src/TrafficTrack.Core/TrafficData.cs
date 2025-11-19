namespace TrafficTrack.Core;

/// <summary>
/// Represents traffic data retrieved from Azure Traffic API
/// </summary>
public class TrafficData
{
    public string? Location { get; set; }
    public DateTime Timestamp { get; set; }
    public double Speed { get; set; }
    public string? Conditions { get; set; }
}
