namespace TrafficTrack.Api.Models;

/// <summary>
/// Rappresenta un'area geografica definita da un rettangolo
/// </summary>
public record BoundingBoxRequest(
    double Lat1,
    double Lon1,
    double Lat2,
    double Lon2
)
{
    public bool IsValid() =>
        Lat1 >= -90 && Lat1 <= 90 &&
        Lat2 >= -90 && Lat2 <= 90 &&
        Lon1 >= -180 && Lon1 <= 180 &&
        Lon2 >= -180 && Lon2 <= 180;

    public double MinLat => Math.Min(Lat1, Lat2);
    public double MaxLat => Math.Max(Lat1, Lat2);
    public double MinLon => Math.Min(Lon1, Lon2);
    public double MaxLon => Math.Max(Lon1, Lon2);
}

/// <summary>
/// Filtri per la ricerca di eventi
/// </summary>
public record EventFilterRequest
{
    public string? EventType { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Severity { get; init; }
}

/// <summary>
/// Informazioni sul traffico in un punto
/// </summary>
public record TrafficInfoResponse
{
    public int Id { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string RoadName { get; init; } = string.Empty;
    public double CurrentSpeed { get; init; }
    public double FreeFlowSpeed { get; init; }
    public double CongestionPercent { get; init; }
    public string CongestionLevel { get; init; } = string.Empty;
    public DateTime RecordedAt { get; init; }
}

/// <summary>
/// Informazioni su un evento stradale
/// </summary>
public record TrafficEventResponse
{
    public int Id { get; init; }
    public string EventId { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string TypeDisplayName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string SeverityDisplayName { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string RoadName { get; init; } = string.Empty;
    public DateTime StartTime { get; init; }
    public DateTime? EndTime { get; init; }
    public DateTime RecordedAt { get; init; }
}

/// <summary>
/// Risposta con dati traffico per un'area
/// </summary>
public record AreaTrafficResponse
{
    public BoundingBoxRequest Area { get; init; } = null!;
    public DateTime QueryTime { get; init; }
    public int TotalFlowRecords { get; init; }
    public double AverageSpeed { get; init; }
    public double AverageCongestion { get; init; }
    public IEnumerable<TrafficInfoResponse> TrafficData { get; init; } = [];
}

/// <summary>
/// Risposta con eventi per un'area
/// </summary>
public record AreaEventsResponse
{
    public BoundingBoxRequest Area { get; init; } = null!;
    public EventFilterRequest? AppliedFilters { get; init; }
    public DateTime QueryTime { get; init; }
    public int TotalEvents { get; init; }
    public Dictionary<string, int> EventsByType { get; init; } = [];
    public IEnumerable<TrafficEventResponse> Events { get; init; } = [];
}

/// <summary>
/// Risposta di errore standard
/// </summary>
public record ApiErrorResponse
{
    public string Error { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? Details { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Richiesta di login
/// </summary>
public record LoginRequest(string Username, string Password);

/// <summary>
/// Risposta di login
/// </summary>
public record LoginResponse(string Token, DateTime Expiration, string Username);

/// <summary>
/// Area salvata dall'utente
/// </summary>
public record SavedAreaRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public double Lat1 { get; init; }
    public double Lon1 { get; init; }
    public double Lat2 { get; init; }
    public double Lon2 { get; init; }
}

public record SavedAreaResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public double Lat1 { get; init; }
    public double Lon1 { get; init; }
    public double Lat2 { get; init; }
    public double Lon2 { get; init; }
    public DateTime CreatedAt { get; init; }
    public string UserId { get; init; } = string.Empty;
}

/// <summary>
/// Tipi di evento supportati
/// </summary>
public static class EventTypes
{
    public const string Accident = "Accident";
    public const string Construction = "Construction";
    public const string Roadblock = "Roadblock";
    public const string TrafficJam = "TrafficJam";
    public const string RoadHazard = "RoadHazard";
    public const string Event = "Event";
    public const string Weather = "Weather";

    public static readonly Dictionary<string, string> DisplayNames = new()
    {
        [Accident] = "Incidente",
        [Construction] = "Lavori Stradali",
        [Roadblock] = "Blocco Stradale",
        [TrafficJam] = "Ingorgo",
        [RoadHazard] = "Pericolo Stradale",
        [Event] = "Manifestazione",
        [Weather] = "Condizioni Meteo"
    };

    public static IEnumerable<string> All => DisplayNames.Keys;
}

/// <summary>
/// Livelli di severità
/// </summary>
public static class SeverityLevels
{
    public const string Low = "Low";
    public const string Moderate = "Moderate";
    public const string Major = "Major";
    public const string Critical = "Critical";

    public static readonly Dictionary<string, string> DisplayNames = new()
    {
        [Low] = "Bassa",
        [Moderate] = "Moderata",
        [Major] = "Alta",
        [Critical] = "Critica"
    };

    public static IEnumerable<string> All => DisplayNames.Keys;
}
