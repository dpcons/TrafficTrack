using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TrafficTrack.Api.Data;
using TrafficTrack.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURAZIONE DATABASE =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=traffictrack_api.db";
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlite(connectionString));

// ===== CONFIGURAZIONE JWT =====
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings
{
    Secret = "TrafficTrackSecretKeyForJwtAuthentication2024!",
    Issuer = "TrafficTrack",
    Audience = "TrafficTrackClients",
    ExpirationMinutes = 60
};
builder.Services.Configure<JwtSettings>(options =>
{
    options.Secret = jwtSettings.Secret;
    options.Issuer = jwtSettings.Issuer;
    options.Audience = jwtSettings.Audience;
    options.ExpirationMinutes = jwtSettings.ExpirationMinutes;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
    };
});

builder.Services.AddAuthorization();

// ===== SERVIZI =====
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpClient<IAzureMapsClient, AzureMapsClient>();
builder.Services.AddScoped<ITrafficService, TrafficService>();

// ===== CONTROLLER E API =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ===== SWAGGER CON AUTENTICAZIONE =====
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TrafficTrack API",
        Version = "v1",
        Description = @"
## API per il monitoraggio del traffico e degli eventi stradali

Questa API permette di:
- **Ottenere informazioni sul traffico** in tempo reale per un'area geografica
- **Visualizzare eventi stradali** (incidenti, lavori, manifestazioni)
- **Filtrare eventi** per tipo, severità e intervallo temporale
- **Salvare aree di interesse** per un accesso rapido

### Autenticazione
L'API utilizza autenticazione JWT. Per ottenere un token:
1. Registra un account con POST `/api/auth/register`
2. Effettua il login con POST `/api/auth/login`
3. Usa il token restituito nell'header `Authorization: Bearer {token}`
",
        Contact = new OpenApiContact
        {
            Name = "TrafficTrack Team",
            Email = "support@traffictrack.example.com"
        }
    });

    // Configura autenticazione JWT in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Inserisci il token JWT nel formato: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Includi commenti XML per documentazione
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ===== INIZIALIZZAZIONE DATABASE =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    await db.Database.EnsureCreatedAsync();
    
    // Crea utente demo se non esiste
    if (!await db.Users.AnyAsync())
    {
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        db.Users.Add(new UserEntity
        {
            Username = "demo",
            PasswordHash = authService.HashPassword("demo123"),
            Email = "demo@traffictrack.example.com"
        });
        await db.SaveChangesAsync();
    }
}

// ===== MIDDLEWARE PIPELINE =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TrafficTrack API v1");
        options.DocumentTitle = "TrafficTrack API Documentation";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ===== ENDPOINT DI HEALTH CHECK =====
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
   .WithName("HealthCheck")
   .WithTags("System");

app.Run();      
