using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrafficTrack.Api.Data;
using TrafficTrack.Api.Models;

namespace TrafficTrack.Api.Controllers;

/// <summary>
/// API per gestire le aree salvate dall'utente
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class SavedAreasController : ControllerBase
{
    private readonly ApiDbContext _db;
    private readonly ILogger<SavedAreasController> _logger;

    public SavedAreasController(ApiDbContext db, ILogger<SavedAreasController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Ottiene tutte le aree salvate dall'utente
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SavedAreaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSavedAreas(CancellationToken ct)
    {
        var userId = GetUserId();
        var areas = await _db.SavedAreas
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

        var result = areas.Select(a => new SavedAreaResponse
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description,
            Lat1 = a.Lat1,
            Lon1 = a.Lon1,
            Lat2 = a.Lat2,
            Lon2 = a.Lon2,
            CreatedAt = a.CreatedAt,
            UserId = a.UserId
        });

        return Ok(result);
    }

    /// <summary>
    /// Ottiene una specifica area salvata
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SavedAreaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSavedArea(int id, CancellationToken ct)
    {
        var userId = GetUserId();
        var area = await _db.SavedAreas.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);

        if (area == null)
            return NotFound(new ApiErrorResponse { Error = "NOT_FOUND", Message = "Area non trovata" });

        return Ok(new SavedAreaResponse
        {
            Id = area.Id,
            Name = area.Name,
            Description = area.Description,
            Lat1 = area.Lat1,
            Lon1 = area.Lon1,
            Lat2 = area.Lat2,
            Lon2 = area.Lon2,
            CreatedAt = area.CreatedAt,
            UserId = area.UserId
        });
    }

    /// <summary>
    /// Salva una nuova area
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SavedAreaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSavedArea([FromBody] SavedAreaRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new ApiErrorResponse
            {
                Error = "INVALID_NAME",
                Message = "Il nome dell'area è obbligatorio"
            });
        }

        var box = new BoundingBoxRequest(request.Lat1, request.Lon1, request.Lat2, request.Lon2);
        if (!box.IsValid())
        {
            return BadRequest(new ApiErrorResponse
            {
                Error = "INVALID_COORDINATES",
                Message = "Le coordinate non sono valide"
            });
        }

        var userId = GetUserId();
        var area = new SavedAreaEntity
        {
            Name = request.Name,
            Description = request.Description,
            Lat1 = request.Lat1,
            Lon1 = request.Lon1,
            Lat2 = request.Lat2,
            Lon2 = request.Lon2,
            UserId = userId
        };

        _db.SavedAreas.Add(area);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} created saved area {AreaId}", userId, area.Id);

        var result = new SavedAreaResponse
        {
            Id = area.Id,
            Name = area.Name,
            Description = area.Description,
            Lat1 = area.Lat1,
            Lon1 = area.Lon1,
            Lat2 = area.Lat2,
            Lon2 = area.Lon2,
            CreatedAt = area.CreatedAt,
            UserId = area.UserId
        };

        return CreatedAtAction(nameof(GetSavedArea), new { id = area.Id }, result);
    }

    /// <summary>
    /// Aggiorna un'area salvata
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SavedAreaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSavedArea(int id, [FromBody] SavedAreaRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var area = await _db.SavedAreas.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);

        if (area == null)
            return NotFound(new ApiErrorResponse { Error = "NOT_FOUND", Message = "Area non trovata" });

        area.Name = request.Name;
        area.Description = request.Description;
        area.Lat1 = request.Lat1;
        area.Lon1 = request.Lon1;
        area.Lat2 = request.Lat2;
        area.Lon2 = request.Lon2;

        await _db.SaveChangesAsync(ct);

        return Ok(new SavedAreaResponse
        {
            Id = area.Id,
            Name = area.Name,
            Description = area.Description,
            Lat1 = area.Lat1,
            Lon1 = area.Lon1,
            Lat2 = area.Lat2,
            Lon2 = area.Lon2,
            CreatedAt = area.CreatedAt,
            UserId = area.UserId
        });
    }

    /// <summary>
    /// Elimina un'area salvata
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSavedArea(int id, CancellationToken ct)
    {
        var userId = GetUserId();
        var area = await _db.SavedAreas.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);

        if (area == null)
            return NotFound(new ApiErrorResponse { Error = "NOT_FOUND", Message = "Area non trovata" });

        _db.SavedAreas.Remove(area);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} deleted saved area {AreaId}", userId, id);

        return NoContent();
    }
}
