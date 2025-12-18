using System.Net.Http.Json;
using System.Text.Json;
using TrafficTrack.Api.Data;
using TrafficTrack.Api.Models;

namespace TrafficTrack.Api.Services;

/// <summary>
/// Client per Azure Maps Traffic API
/// </summary>
public interface IAzureMapsClient
{
    Task<IEnumerable<TrafficFlowEntity>> GetTrafficFlowAsync(BoundingBoxRequest area, CancellationToken ct = default);
    Task<IEnumerable<TrafficEventEntity>> GetTrafficIncidentsAsync(BoundingBoxRequest area, CancellationToken ct = default);
}

public class AzureMapsClient : IAzureMapsClient
{
    private readonly HttpClient _httpClient;
    private readonly string _subscriptionKey;
    private readonly ILogger<AzureMapsClient> _logger;

    public AzureMapsClient(HttpClient httpClient, IConfiguration config, ILogger<AzureMapsClient> logger)
    {
        _httpClient = httpClient;
        _subscriptionKey = config["AzureMaps:SubscriptionKey"] ?? string.Empty;
        _logger = logger;
    }

    public async Task<IEnumerable<TrafficFlowEntity>> GetTrafficFlowAsync(BoundingBoxRequest area, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_subscriptionKey))
        {
            _logger.LogWarning("Azure Maps subscription key not configured");
            return [];
        }

        try
        {
            // Azure Maps Traffic Flow Segment API
            // https://docs.microsoft.com/en-us/rest/api/maps/traffic/get-traffic-flow-segment
            var bbox = $"{area.MinLon},{area.MinLat},{area.MaxLon},{area.MaxLat}";
            var url = $"https://atlas.microsoft.com/traffic/flow/segment/json" +
                     $"?api-version=1.0&subscription-key={_subscriptionKey}" +
                     $"&style=absolute&zoom=10" +
                     $"&query={area.MinLat},{area.MinLon}";

            var response = await _httpClient.GetAsync(url, ct);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Azure Maps API returned {StatusCode}", response.StatusCode);
                return [];
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            var data = JsonSerializer.Deserialize<AzureMapsFlowResponse>(content, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            if (data?.FlowSegmentData == null)
                return [];

            var now = DateTime.UtcNow;
            return new[]
            {
                new TrafficFlowEntity
                {
                    Latitude = data.FlowSegmentData.Coordinates?.Coordinate?.FirstOrDefault()?.Latitude ?? area.MinLat,
                    Longitude = data.FlowSegmentData.Coordinates?.Coordinate?.FirstOrDefault()?.Longitude ?? area.MinLon,
                    RoadName = data.FlowSegmentData.Frc ?? "Unknown",
                    CurrentSpeed = data.FlowSegmentData.CurrentSpeed ?? 0,
                    FreeFlowSpeed = data.FlowSegmentData.FreeFlowSpeed ?? 0,
                    CurrentTravelTime = data.FlowSegmentData.CurrentTravelTime ?? 0,
                    FreeFlowTravelTime = data.FlowSegmentData.FreeFlowTravelTime ?? 0,
                    Confidence = data.FlowSegmentData.Confidence ?? 0.5,
                    RecordedAt = now
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Azure Maps Traffic Flow API");
            return [];
        }
    }

    public async Task<IEnumerable<TrafficEventEntity>> GetTrafficIncidentsAsync(BoundingBoxRequest area, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_subscriptionKey))
        {
            _logger.LogWarning("Azure Maps subscription key not configured");
            return [];
        }

        try
        {
            // Azure Maps Traffic Incident Detail API
            // https://docs.microsoft.com/en-us/rest/api/maps/traffic/get-traffic-incident-detail
            var bbox = $"{area.MinLon},{area.MinLat},{area.MaxLon},{area.MaxLat}";
            var url = $"https://atlas.microsoft.com/traffic/incident/detail/json" +
                     $"?api-version=1.0&subscription-key={_subscriptionKey}" +
                     $"&bbox={bbox}&language=it-IT&projection=EPSG4326";

            var response = await _httpClient.GetAsync(url, ct);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Azure Maps Incident API returned {StatusCode}", response.StatusCode);
                return [];
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            var data = JsonSerializer.Deserialize<AzureMapsIncidentResponse>(content, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            if (data?.Incidents == null)
                return [];

            var now = DateTime.UtcNow;
            var events = new List<TrafficEventEntity>();

            foreach (var incident in data.Incidents)
            {
                var coords = incident.Geometry?.Coordinates?.FirstOrDefault();
                if (coords == null || coords.Count < 2) continue;

                events.Add(new TrafficEventEntity
                {
                    EventId = incident.Id ?? Guid.NewGuid().ToString(),
                    Type = MapIconCategoryToType(incident.Properties?.IconCategory),
                    Description = incident.Properties?.Description ?? "Evento stradale",
                    Severity = MapMagnitudeToSeverity(incident.Properties?.MagnitudeOfDelay),
                    Latitude = coords[1], // GeoJSON: [lon, lat]
                    Longitude = coords[0],
                    RoadName = incident.Properties?.RoadNumbers?.FirstOrDefault() ?? "Strada sconosciuta",
                    StartTime = incident.Properties?.StartTime ?? now,
                    EndTime = incident.Properties?.EndTime,
                    RecordedAt = now
                });
            }

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Azure Maps Incident API");
            return [];
        }
    }

    private static string MapIconCategoryToType(int? category) => category switch
    {
        1 or 2 or 3 or 4 => EventTypes.Accident,
        5 or 6 => EventTypes.RoadHazard,
        7 or 8 or 9 => EventTypes.Construction,
        10 or 11 => EventTypes.Roadblock,
        12 or 13 or 14 => EventTypes.TrafficJam,
        _ => EventTypes.Event
    };

    private static string MapMagnitudeToSeverity(int? magnitude) => magnitude switch
    {
        0 => SeverityLevels.Low,
        1 => SeverityLevels.Moderate,
        2 => SeverityLevels.Major,
        3 or 4 => SeverityLevels.Critical,
        _ => SeverityLevels.Low
    };
}

// DTOs per Azure Maps API
public class AzureMapsFlowResponse
{
    public FlowSegmentData? FlowSegmentData { get; set; }
}

public class FlowSegmentData
{
    public string? Frc { get; set; }
    public double? CurrentSpeed { get; set; }
    public double? FreeFlowSpeed { get; set; }
    public double? CurrentTravelTime { get; set; }
    public double? FreeFlowTravelTime { get; set; }
    public double? Confidence { get; set; }
    public CoordinatesData? Coordinates { get; set; }
}

public class CoordinatesData
{
    public List<CoordinatePoint>? Coordinate { get; set; }
}

public class CoordinatePoint
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class AzureMapsIncidentResponse
{
    public List<IncidentData>? Incidents { get; set; }
}

public class IncidentData
{
    public string? Id { get; set; }
    public string? Type { get; set; }
    public IncidentGeometry? Geometry { get; set; }
    public IncidentProperties? Properties { get; set; }
}

public class IncidentGeometry
{
    public List<List<double>>? Coordinates { get; set; }
}

public class IncidentProperties
{
    public int? IconCategory { get; set; }
    public int? MagnitudeOfDelay { get; set; }
    public string? Description { get; set; }
    public List<string>? RoadNumbers { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}
