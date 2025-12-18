using Microsoft.EntityFrameworkCore;
using TrafficTrack.Api.Data;
using TrafficTrack.Api.Models;

namespace TrafficTrack.Api.Services;

public interface ITrafficService
{
    Task<AreaTrafficResponse> GetTrafficInAreaAsync(BoundingBoxRequest area, CancellationToken ct = default);
    Task<AreaEventsResponse> GetEventsInAreaAsync(BoundingBoxRequest area, EventFilterRequest? filters = null, CancellationToken ct = default);
    Task<IEnumerable<TrafficEventResponse>> GetRecentEventsAsync(int hours = 24, CancellationToken ct = default);
    Task RefreshDataAsync(BoundingBoxRequest area, CancellationToken ct = default);
}

public class TrafficService : ITrafficService
{
    private readonly ApiDbContext _db;
    private readonly ILogger<TrafficService> _logger;
    private readonly IAzureMapsClient? _azureMapsClient;
    private readonly bool _useMockData;

    public TrafficService(
        ApiDbContext db, 
        ILogger<TrafficService> logger, 
        IConfiguration config,
        IAzureMapsClient? azureMapsClient = null)
    {
        _db = db;
        _logger = logger;
        _azureMapsClient = azureMapsClient;
        _useMockData = config.GetValue("AzureMaps:UseMockData", true);
    }

    public async Task<AreaTrafficResponse> GetTrafficInAreaAsync(BoundingBoxRequest area, CancellationToken ct = default)
    {
        var query = _db.TrafficFlows
            .Where(f => f.Latitude >= area.MinLat && f.Latitude <= area.MaxLat)
            .Where(f => f.Longitude >= area.MinLon && f.Longitude <= area.MaxLon)
            .OrderByDescending(f => f.RecordedAt);

        var data = await query.Take(1000).ToListAsync(ct);

        // Se non ci sono dati recenti, prova Azure Maps o genera mock
        if (!data.Any() || data.Max(d => d.RecordedAt) < DateTime.UtcNow.AddMinutes(-30))
        {
            await RefreshDataAsync(area, ct);
            data = await query.Take(1000).ToListAsync(ct);
        }

        var trafficData = data.Select(f => new TrafficInfoResponse
        {
            Id = f.Id,
            Latitude = f.Latitude,
            Longitude = f.Longitude,
            RoadName = f.RoadName,
            CurrentSpeed = Math.Round(f.CurrentSpeed, 1),
            FreeFlowSpeed = Math.Round(f.FreeFlowSpeed, 1),
            CongestionPercent = Math.Round(f.FreeFlowSpeed > 0 ? Math.Max(0, (1 - f.CurrentSpeed / f.FreeFlowSpeed) * 100) : 0, 1),
            CongestionLevel = GetCongestionLevel(f.CurrentSpeed, f.FreeFlowSpeed),
            RecordedAt = f.RecordedAt
        }).ToList();

        return new AreaTrafficResponse
        {
            Area = area,
            QueryTime = DateTime.UtcNow,
            TotalFlowRecords = trafficData.Count,
            AverageSpeed = Math.Round(trafficData.Any() ? trafficData.Average(t => t.CurrentSpeed) : 0, 1),
            AverageCongestion = Math.Round(trafficData.Any() ? trafficData.Average(t => t.CongestionPercent) : 0, 1),
            TrafficData = trafficData
        };
    }

    public async Task<AreaEventsResponse> GetEventsInAreaAsync(BoundingBoxRequest area, EventFilterRequest? filters = null, CancellationToken ct = default)
    {
        var query = _db.TrafficEvents
            .Where(e => e.Latitude >= area.MinLat && e.Latitude <= area.MaxLat)
            .Where(e => e.Longitude >= area.MinLon && e.Longitude <= area.MaxLon);

        if (!string.IsNullOrEmpty(filters?.EventType))
            query = query.Where(e => e.Type == filters.EventType);

        if (!string.IsNullOrEmpty(filters?.Severity))
            query = query.Where(e => e.Severity == filters.Severity);

        if (filters?.FromDate.HasValue == true)
            query = query.Where(e => e.StartTime >= filters.FromDate.Value);

        if (filters?.ToDate.HasValue == true)
            query = query.Where(e => e.StartTime <= filters.ToDate.Value);

        var data = await query.OrderByDescending(e => e.StartTime).Take(500).ToListAsync(ct);

        // Se non ci sono dati recenti, prova Azure Maps o genera mock
        if (!data.Any() || data.Max(d => d.RecordedAt) < DateTime.UtcNow.AddMinutes(-30))
        {
            await RefreshDataAsync(area, ct);
            data = await query.OrderByDescending(e => e.StartTime).Take(500).ToListAsync(ct);
        }

        var events = data.Select(e => new TrafficEventResponse
        {
            Id = e.Id,
            EventId = e.EventId,
            Type = e.Type,
            TypeDisplayName = EventTypes.DisplayNames.GetValueOrDefault(e.Type, e.Type),
            Description = e.Description,
            Severity = e.Severity,
            SeverityDisplayName = SeverityLevels.DisplayNames.GetValueOrDefault(e.Severity, e.Severity),
            Latitude = e.Latitude,
            Longitude = e.Longitude,
            RoadName = e.RoadName,
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            RecordedAt = e.RecordedAt
        }).ToList();

        var eventsByType = events.GroupBy(e => e.Type)
            .ToDictionary(g => g.Key, g => g.Count());

        return new AreaEventsResponse
        {
            Area = area,
            AppliedFilters = filters,
            QueryTime = DateTime.UtcNow,
            TotalEvents = events.Count,
            EventsByType = eventsByType,
            Events = events
        };
    }

    public async Task<IEnumerable<TrafficEventResponse>> GetRecentEventsAsync(int hours = 24, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddHours(-hours);
        var data = await _db.TrafficEvents
            .Where(e => e.RecordedAt >= since)
            .OrderByDescending(e => e.StartTime)
            .Take(100)
            .ToListAsync(ct);

        return data.Select(e => new TrafficEventResponse
        {
            Id = e.Id,
            EventId = e.EventId,
            Type = e.Type,
            TypeDisplayName = EventTypes.DisplayNames.GetValueOrDefault(e.Type, e.Type),
            Description = e.Description,
            Severity = e.Severity,
            SeverityDisplayName = SeverityLevels.DisplayNames.GetValueOrDefault(e.Severity, e.Severity),
            Latitude = e.Latitude,
            Longitude = e.Longitude,
            RoadName = e.RoadName,
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            RecordedAt = e.RecordedAt
        });
    }

    public async Task RefreshDataAsync(BoundingBoxRequest area, CancellationToken ct = default)
    {
        _logger.LogInformation("Refreshing data for area {MinLat},{MinLon} to {MaxLat},{MaxLon}", 
            area.MinLat, area.MinLon, area.MaxLat, area.MaxLon);

        // Prova prima con Azure Maps se disponibile e non in modalità mock
        if (!_useMockData && _azureMapsClient != null)
        {
            try
            {
                var trafficFlows = await _azureMapsClient.GetTrafficFlowAsync(area, ct);
                if (trafficFlows.Any())
                {
                    await _db.TrafficFlows.AddRangeAsync(trafficFlows, ct);
                    _logger.LogInformation("Added {Count} traffic flows from Azure Maps", trafficFlows.Count());
                }

                var incidents = await _azureMapsClient.GetTrafficIncidentsAsync(area, ct);
                if (incidents.Any())
                {
                    await _db.TrafficEvents.AddRangeAsync(incidents, ct);
                    _logger.LogInformation("Added {Count} incidents from Azure Maps", incidents.Count());
                }

                await _db.SaveChangesAsync(ct);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Azure Maps call failed, falling back to mock data");
            }
        }

        // Fallback a dati mock
        await GenerateMockTrafficData(area, ct);
        await GenerateMockEventData(area, ct);
    }

    private async Task GenerateMockTrafficData(BoundingBoxRequest area, CancellationToken ct)
    {
        var random = new Random();
        var now = DateTime.UtcNow;
        var roads = new[] { "Via Roma", "Corso Italia", "Viale Europa", "Via Garibaldi", "Piazza Duomo", "Tangenziale Nord", "SS35", "Via Milano", "Corso Cavour" };

        var flows = new List<TrafficFlowEntity>();
        for (int i = 0; i < 15; i++)
        {
            var lat = area.MinLat + random.NextDouble() * (area.MaxLat - area.MinLat);
            var lon = area.MinLon + random.NextDouble() * (area.MaxLon - area.MinLon);
            var freeFlow = 50 + random.Next(50);
            var current = freeFlow * (0.3 + random.NextDouble() * 0.7);

            flows.Add(new TrafficFlowEntity
            {
                Latitude = lat,
                Longitude = lon,
                RoadName = roads[random.Next(roads.Length)],
                CurrentSpeed = current,
                FreeFlowSpeed = freeFlow,
                Confidence = 0.8 + random.NextDouble() * 0.2,
                RecordedAt = now
            });
        }

        await _db.TrafficFlows.AddRangeAsync(flows, ct);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Generated {Count} mock traffic flows", flows.Count);
    }

    private async Task GenerateMockEventData(BoundingBoxRequest area, CancellationToken ct)
    {
        var random = new Random();
        var now = DateTime.UtcNow;
        var types = EventTypes.All.ToArray();
        var severities = SeverityLevels.All.ToArray();
        var roads = new[] { "Via Roma", "Corso Italia", "Viale Europa", "Via Garibaldi", "SS35", "Tangenziale" };

        var events = new List<TrafficEventEntity>();
        for (int i = 0; i < 8; i++)
        {
            var lat = area.MinLat + random.NextDouble() * (area.MaxLat - area.MinLat);
            var lon = area.MinLon + random.NextDouble() * (area.MaxLon - area.MinLon);
            var type = types[random.Next(types.Length)];

            events.Add(new TrafficEventEntity
            {
                EventId = Guid.NewGuid().ToString(),
                Type = type,
                Description = $"{EventTypes.DisplayNames[type]} rilevato sulla strada",
                Severity = severities[random.Next(severities.Length)],
                Latitude = lat,
                Longitude = lon,
                RoadName = roads[random.Next(roads.Length)],
                StartTime = now.AddHours(-random.Next(1, 24)),
                EndTime = random.Next(2) == 0 ? now.AddHours(random.Next(1, 12)) : null,
                RecordedAt = now
            });
        }

        await _db.TrafficEvents.AddRangeAsync(events, ct);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Generated {Count} mock events", events.Count);
    }

    private static string GetCongestionLevel(double current, double freeFlow)
    {
        if (freeFlow == 0) return "Sconosciuto";
        var ratio = current / freeFlow;
        return ratio switch
        {
            >= 0.8 => "Scorrevole",
            >= 0.5 => "Moderato",
            >= 0.25 => "Intenso",
            _ => "Bloccato"
        };
    }
}
