namespace TrafficTrack.App.Models;

/// <summary>
/// Risposta standard dell'API con errore
/// </summary>
public record ApiError(string Error, string Message, string? Details = null);

/// <summary>
/// Risposta di login
/// </summary>
public record LoginResponse(string Token, DateTime Expiration, string Username);

/// <summary>
/// Informazioni sul traffico
/// </summary>
public partial record TrafficInfo
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
/// Risposta con dati traffico
/// </summary>
public record AreaTrafficResponse
{
    public BoundingBox? Area { get; init; }
    public DateTime QueryTime { get; init; }
    public int TotalFlowRecords { get; init; }
    public double AverageSpeed { get; init; }
    public double AverageCongestion { get; init; }
    public List<TrafficInfo> TrafficData { get; init; } = [];
}

/// <summary>
/// Evento stradale
/// </summary>
public partial record TrafficEvent
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
/// Risposta con eventi
/// </summary>
public record AreaEventsResponse
{
    public BoundingBox? Area { get; init; }
    public DateTime QueryTime { get; init; }
    public int TotalEvents { get; init; }
    public Dictionary<string, int> EventsByType { get; init; } = [];
    public List<TrafficEvent> Events { get; init; } = [];
}

/// <summary>
/// Area geografica
/// </summary>
public record BoundingBox(double Lat1, double Lon1, double Lat2, double Lon2)
{
    public double MinLat => Math.Min(Lat1, Lat2);
    public double MaxLat => Math.Max(Lat1, Lat2);
    public double MinLon => Math.Min(Lon1, Lon2);
    public double MaxLon => Math.Max(Lon1, Lon2);

    public bool IsValid() =>
        Lat1 >= -90 && Lat1 <= 90 &&
        Lat2 >= -90 && Lat2 <= 90 &&
        Lon1 >= -180 && Lon1 <= 180 &&
        Lon2 >= -180 && Lon2 <= 180;
}

/// <summary>
/// Area salvata
/// </summary>
public partial record SavedArea
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public double Lat1 { get; init; }
    public double Lon1 { get; init; }
    public double Lat2 { get; init; }
    public double Lon2 { get; init; }
    public DateTime CreatedAt { get; init; }

    public BoundingBox ToBoundingBox() => new(Lat1, Lon1, Lat2, Lon2);
}

/// <summary>
/// Richiesta per salvare un'area
/// </summary>
public record SaveAreaRequest(string Name, string? Description, double Lat1, double Lon1, double Lat2, double Lon2);

/// <summary>
/// Tipi di evento
/// </summary>
public static class EventTypes
{
    public static readonly Dictionary<string, string> DisplayNames = new()
    {
        ["Accident"] = "Incidente",
        ["Construction"] = "Lavori Stradali",
        ["Roadblock"] = "Blocco Stradale",
        ["TrafficJam"] = "Ingorgo",
        ["RoadHazard"] = "Pericolo Stradale",
        ["Event"] = "Manifestazione",
        ["Weather"] = "Condizioni Meteo"
    };
}

/// <summary>
/// Livelli di severità
/// </summary>
public static class SeverityLevels
{
    public static readonly Dictionary<string, string> DisplayNames = new()
    {
        ["Low"] = "Bassa",
        ["Moderate"] = "Moderata",
        ["Major"] = "Alta",
        ["Critical"] = "Critica"
    };
}
