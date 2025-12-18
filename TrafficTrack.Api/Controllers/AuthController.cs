using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrafficTrack.Api.Data;
using TrafficTrack.Api.Models;
using TrafficTrack.Api.Services;

namespace TrafficTrack.Api.Controllers;

/// <summary>
/// API per autenticazione e gestione utenti
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ApiDbContext _db;
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ApiDbContext db, IAuthService authService, ILogger<AuthController> logger)
    {
        _db = db;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Effettua il login e restituisce un token JWT
    /// </summary>
    /// <param name="request">Credenziali di accesso</param>
    /// <returns>Token JWT</returns>
    /// <response code="200">Login effettuato con successo</response>
    /// <response code="401">Credenziali non valide</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null || !_authService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new ApiErrorResponse
            {
                Error = "INVALID_CREDENTIALS",
                Message = "Username o password non validi"
            });
        }

        var token = _authService.GenerateToken(user);
        var expiration = DateTime.UtcNow.AddMinutes(60);

        _logger.LogInformation("User {Username} logged in", user.Username);

        return Ok(new LoginResponse(token, expiration, user.Username));
    }

    /// <summary>
    /// Registra un nuovo utente
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 3)
        {
            return BadRequest(new ApiErrorResponse
            {
                Error = "INVALID_USERNAME",
                Message = "Lo username deve avere almeno 3 caratteri"
            });
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            return BadRequest(new ApiErrorResponse
            {
                Error = "INVALID_PASSWORD",
                Message = "La password deve avere almeno 6 caratteri"
            });
        }

        if (await _db.Users.AnyAsync(u => u.Username == request.Username))
        {
            return BadRequest(new ApiErrorResponse
            {
                Error = "USERNAME_EXISTS",
                Message = "Username già in uso"
            });
        }

        var user = new UserEntity
        {
            Username = request.Username,
            PasswordHash = _authService.HashPassword(request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _authService.GenerateToken(user);
        var expiration = DateTime.UtcNow.AddMinutes(60);

        _logger.LogInformation("New user {Username} registered", user.Username);

        return Ok(new LoginResponse(token, expiration, user.Username));
    }

    /// <summary>
    /// Verifica se il token è valido
    /// </summary>
    [HttpGet("verify")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Verify()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = User.FindFirstValue(ClaimTypes.Name);
        return Ok(new { UserId = userId, Username = username, Valid = true });
    }
}
