using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrafficTrack.Api.Models;
using TrafficTrack.Api.Services;

namespace TrafficTrack.Api.Controllers;

/// <summary>
/// API per ottenere informazioni sul traffico
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TrafficController : ControllerBase
{
    private readonly ITrafficService _trafficService;
    private readonly ILogger<TrafficController> _logger;

    public TrafficController(ITrafficService trafficService, ILogger<TrafficController> logger)
    {
        _trafficService = trafficService;
        _logger = logger;
    }

    /// <summary>
    /// Ottiene le informazioni sul traffico attuale in un'area specificata
    /// </summary>
    /// <param name="lat1">Latitudine primo angolo</param>
    /// <param name="lon1">Longitudine primo angolo</param>
    /// <param name="lat2">Latitudine secondo angolo (opposto)</param>
    /// <param name="lon2">Longitudine secondo angolo (opposto)</param>
    /// <param name="ct">Token di cancellazione</param>
    /// <returns>Dati sul traffico nell'area</returns>
    /// <response code="200">Dati traffico restituiti con successo</response>
    /// <response code="400">Coordinate non valide</response>
    /// <response code="401">Non autorizzato</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(AreaTrafficResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTraffic(
        [FromQuery] double lat1,
        [FromQuery] double lon1,
        [FromQuery] double lat2,
        [FromQuery] double lon2,
        CancellationToken ct)
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

        try
        {
            var result = await _trafficService.GetTrafficInAreaAsync(area, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero dati traffico");
            return StatusCode(500, new ApiErrorResponse
            {
                Error = "INTERNAL_ERROR",
                Message = "Errore nel recupero dei dati sul traffico"
            });
        }
    }

    /// <summary>
    /// Aggiorna i dati del traffico per un'area (richiede i dati più recenti)
    /// </summary>
    [HttpPost("refresh")]
    [Authorize]
    [ProducesResponseType(typeof(AreaTrafficResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshTraffic(
        [FromQuery] double lat1,
        [FromQuery] double lon1,
        [FromQuery] double lat2,
        [FromQuery] double lon2,
        CancellationToken ct)
    {
        var area = new BoundingBoxRequest(lat1, lon1, lat2, lon2);

        if (!area.IsValid())
        {
            return BadRequest(new ApiErrorResponse
            {
                Error = "INVALID_COORDINATES",
                Message = "Le coordinate fornite non sono valide"
            });
        }

        await _trafficService.RefreshDataAsync(area, ct);
        var result = await _trafficService.GetTrafficInAreaAsync(area, ct);
        return Ok(result);
    }
}
