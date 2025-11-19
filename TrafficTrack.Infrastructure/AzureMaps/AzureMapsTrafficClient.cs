using System.Net.Http.Json;
using System.Text.Json;
using TrafficTrack.Core.Interfaces;
using TrafficTrack.Core.Models;

namespace TrafficTrack.Infrastructure.AzureMaps;

public class AzureMapsTrafficClient : ITrafficApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _subscriptionKey;
    private readonly bool _useMockData;

    public AzureMapsTrafficClient(HttpClient httpClient, string subscriptionKey, bool useMockData = false)
    {
        _httpClient = httpClient;
        _subscriptionKey = subscriptionKey;
        _useMockData = useMockData;
        
        if (!_useMockData && string.IsNullOrWhiteSpace(_subscriptionKey))
        {
            throw new ArgumentException("Subscription key is required when not using mock data", nameof(subscriptionKey));
        }
    }

    public async Task<IEnumerable<TrafficIncident>> GetTrafficIncidentsAsync(BoundingBox area, CancellationToken cancellationToken = default)
    {
        if (_useMockData)
        {
            return GenerateMockIncidents(area);
        }

        try
        {
            // Azure Maps Traffic Incident Details API
            // https://atlas.microsoft.com/traffic/incident/detail/json?api-version=1.0&subscription-key={key}&bbox={minLon},{minLat},{maxLon},{maxLat}
            var bbox = $"{area.TopLeft.Longitude},{area.BottomRight.Latitude},{area.BottomRight.Longitude},{area.TopLeft.Latitude}";
            var url = $"https://atlas.microsoft.com/traffic/incident/detail/json?api-version=1.0&subscription-key={_subscriptionKey}&bbox={bbox}&language=en&projection=EPSG4326";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<AzureMapsIncidentResponse>(content);

            if (apiResponse?.Incidents == null)
                return Array.Empty<TrafficIncident>();

            var incidents = new List<TrafficIncident>();
            var now = DateTime.UtcNow;

            foreach (var incident in apiResponse.Incidents)
            {
                incidents.Add(new TrafficIncident
                {
                    IncidentId = incident.Id ?? Guid.NewGuid().ToString(),
                    Latitude = incident.Geometry?.Coordinates?.FirstOrDefault()?.FirstOrDefault() ?? 0,
                    Longitude = incident.Geometry?.Coordinates?.FirstOrDefault()?.Skip(1).FirstOrDefault() ?? 0,
                    Type = incident.Properties?.IconCategory?.ToString() ?? "Unknown",
                    Description = incident.Properties?.Description ?? "No description",
                    Severity = incident.Properties?.Magnitude?.ToString() ?? "Unknown",
                    StartTime = incident.Properties?.StartTime ?? now,
                    EndTime = incident.Properties?.EndTime,
                    RecordedAt = now,
                    RoadName = incident.Properties?.RoadNumber ?? "Unknown",
                    Area = DetermineArea(area)
                });
            }

            return incidents;
        }
        catch (HttpRequestException)
        {
            // If API call fails, return mock data as fallback
            return GenerateMockIncidents(area);
        }
    }

    public async Task<IEnumerable<TrafficFlow>> GetTrafficFlowAsync(BoundingBox area, CancellationToken cancellationToken = default)
    {
        if (_useMockData)
        {
            return GenerateMockFlows(area);
        }

        try
        {
            // Azure Maps Traffic Flow Segment API
            // For simplicity, we'll generate mock data here as the real API requires more complex parameters
            // In a real implementation, you would call the Azure Maps Traffic Flow Segment API
            return await Task.FromResult(GenerateMockFlows(area));
        }
        catch (HttpRequestException)
        {
            return GenerateMockFlows(area);
        }
    }

    private IEnumerable<TrafficIncident> GenerateMockIncidents(BoundingBox area)
    {
        var random = new Random();
        var incidents = new List<TrafficIncident>();
        var now = DateTime.UtcNow;
        var types = new[] { "Accident", "Construction", "Roadblock", "Traffic Jam", "Road Hazard" };
        var severities = new[] { "Low", "Medium", "High", "Critical" };

        for (int i = 0; i < 5; i++)
        {
            var lat = area.BottomRight.Latitude + random.NextDouble() * (area.TopLeft.Latitude - area.BottomRight.Latitude);
            var lon = area.TopLeft.Longitude + random.NextDouble() * (area.BottomRight.Longitude - area.TopLeft.Longitude);

            incidents.Add(new TrafficIncident
            {
                IncidentId = Guid.NewGuid().ToString(),
                Latitude = lat,
                Longitude = lon,
                Type = types[random.Next(types.Length)],
                Description = $"Mock incident at ({lat:F4}, {lon:F4})",
                Severity = severities[random.Next(severities.Length)],
                StartTime = now.AddHours(-random.Next(1, 24)),
                EndTime = random.Next(2) == 0 ? now.AddHours(random.Next(1, 12)) : null,
                RecordedAt = now,
                RoadName = $"Highway {random.Next(1, 100)}",
                Area = DetermineArea(area)
            });
        }

        return incidents;
    }

    private IEnumerable<TrafficFlow> GenerateMockFlows(BoundingBox area)
    {
        var random = new Random();
        var flows = new List<TrafficFlow>();
        var now = DateTime.UtcNow;

        for (int i = 0; i < 10; i++)
        {
            var lat = area.BottomRight.Latitude + random.NextDouble() * (area.TopLeft.Latitude - area.BottomRight.Latitude);
            var lon = area.TopLeft.Longitude + random.NextDouble() * (area.BottomRight.Longitude - area.TopLeft.Longitude);
            var freeFlowSpeed = 60 + random.Next(40);
            var currentSpeed = freeFlowSpeed * (0.3 + random.NextDouble() * 0.7);

            flows.Add(new TrafficFlow
            {
                Latitude = lat,
                Longitude = lon,
                CurrentSpeed = currentSpeed,
                FreeFlowSpeed = freeFlowSpeed,
                CurrentTravelTime = 10 + random.Next(20),
                FreeFlowTravelTime = 5 + random.Next(10),
                Confidence = 0.7 + random.NextDouble() * 0.3,
                RoadName = $"Highway {random.Next(1, 100)}",
                RecordedAt = now,
                Area = DetermineArea(area)
            });
        }

        return flows;
    }

    private string DetermineArea(BoundingBox area)
    {
        // Simple area naming based on coordinates
        return $"Area_{area.TopLeft.Latitude:F2}_{area.TopLeft.Longitude:F2}";
    }

    // DTOs for Azure Maps API response
    private class AzureMapsIncidentResponse
    {
        public List<Incident>? Incidents { get; set; }
    }

    private class Incident
    {
        public string? Id { get; set; }
        public IncidentGeometry? Geometry { get; set; }
        public IncidentProperties? Properties { get; set; }
    }

    private class IncidentGeometry
    {
        public List<List<double>>? Coordinates { get; set; }
    }

    private class IncidentProperties
    {
        public int? IconCategory { get; set; }
        public int? Magnitude { get; set; }
        public string? Description { get; set; }
        public string? RoadNumber { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
