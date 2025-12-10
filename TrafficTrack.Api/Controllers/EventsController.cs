using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrafficTrack.Api.Models;
using TrafficTrack.Api.Services;

namespace TrafficTrack.Api.Controllers;

/// <summary>
/// API per ottenere informazioni sugli eventi stradali
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EventsController : ControllerBase
{
    private readonly ITrafficService _trafficService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(ITrafficService trafficService, ILogger<EventsController> logger)
    {
        _trafficService = trafficService;
        _logger = logger;
    }

    /// <summary>
    /// Ottiene gli eventi stradali in un'area specificata
    /// </summary>
    /// <param name="lat1">Latitudine primo angolo</param>
    /// <param name="lon1">Longitudine primo angolo</param>
    /// <param name="lat2">Latitudine secondo angolo (opposto)</param>
    /// <param name="lon2">Longitudine secondo angolo (opposto)</param>
    /// <param name="eventType">Tipo di evento (opzionale): Accident, Construction, Roadblock, TrafficJam, RoadHazard, Event, Weather</param>
    /// <param name="severity">Severità (opzionale): Low, Moderate, Major, Critical</param>
    /// <param name="fromDate">Data inizio intervallo (opzionale)</param>
    /// <param name="toDate">Data fine intervallo (opzionale)</param>
    /// <param name="ct">Token di cancellazione</param>
    /// <returns>Eventi nell'area specificata</returns>
    /// <response code="200">Eventi restituiti con successo</response>
    /// <response code="400">Coordinate o filtri non validi</response>
    /// <response code="401">Non autorizzato</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(AreaEventsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetEvents(
        [FromQuery] double lat1,
        [FromQuery] double lon1,
        [FromQuery] double lat2,
        [FromQuery] double lon2,
        [FromQuery] string? eventType = null,
        [FromQuery] string? severity = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var area = new BoundingBoxRequest(lat1, lon1, lat2, lon2);

        if (!area.IsValid())
        {
            return BadRequest(new ApiErrorResponse
            {
                Error = "INVALID_COORDINATES",
                Message = "Le coordinate fornite non sono valide. Lat: -90 a 90, Lon: -180 a 180"
            });
        }

        if (!string.IsNullOrEmpty(eventType) && !EventTypes.All.Contains(eventType))
        {
            return BadRequest(new ApiErrorResponse
            {
                Error = "INVALID_EVENT_TYPE",
                Message = $"Tipo evento non valido. Valori ammessi: {string.Join(", ", EventTypes.All)}"
            });
        }

        if (!string.IsNullOrEmpty(severity) && !SeverityLevels.All.Contains(severity))
        {
            return BadRequest(new ApiErrorResponse
            {
                Error = "INVALID_SEVERITY",
                Message = $"Severità non valida. Valori ammessi: {string.Join(", ", SeverityLevels.All)}"
            });
        }

        var filters = new EventFilterRequest
        {
            EventType = eventType,
            Severity = severity,
            FromDate = fromDate,
            ToDate = toDate
        };

        try
        {
            var result = await _trafficService.GetEventsInAreaAsync(area, filters, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero eventi");
            return StatusCode(500, new ApiErrorResponse
            {
                Error = "INTERNAL_ERROR",
                Message = "Errore nel recupero degli eventi"
            });
        }
    }

    /// <summary>
    /// Ottiene gli eventi recenti (ultime N ore)
    /// </summary>
    /// <param name="hours">Numero di ore da considerare (default 24)</param>
    /// <param name="ct">Token di cancellazione</param>
    /// <returns>Eventi recenti</returns>
    [HttpGet("recent")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<TrafficEventResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentEvents(
        [FromQuery] int hours = 24,
        CancellationToken ct = default)
    {
        if (hours < 1 || hours > 168) // max 1 settimana
        {
            return BadRequest(new ApiErrorResponse
            {
                Error = "INVALID_HOURS",
                Message = "Il parametro hours deve essere tra 1 e 168"
            });
        }

        var result = await _trafficService.GetRecentEventsAsync(hours, ct);
        return Ok(result);
    }

    /// <summary>
    /// Ottiene i tipi di evento disponibili
    /// </summary>
    [HttpGet("types")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public IActionResult GetEventTypes()
    {
        return Ok(EventTypes.DisplayNames);
    }

    /// <summary>
    /// Ottiene i livelli di severità disponibili
    /// </summary>
    [HttpGet("severities")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public IActionResult GetSeverityLevels()
    {
        return Ok(SeverityLevels.DisplayNames);
    }
}
