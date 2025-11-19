using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrafficTrack.Core.Interfaces;
using TrafficTrack.Core.Models;
using TrafficTrack.Infrastructure.AzureMaps;
using TrafficTrack.Infrastructure.Data;
using TrafficTrack.Services.Implementation;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Setup DI container
var services = new ServiceCollection();

// Add DbContext
var connectionString = configuration["Database:ConnectionString"] ?? "Data Source=traffictrack.db";
services.AddDbContext<TrafficDbContext>(options =>
    options.UseSqlite(connectionString));

// Add HttpClient
services.AddHttpClient<ITrafficApiClient, AzureMapsTrafficClient>((sp, client) =>
{
    client.BaseAddress = new Uri("https://atlas.microsoft.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Configure AzureMapsTrafficClient
services.AddScoped<ITrafficApiClient>(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
    var subscriptionKey = configuration["AzureMaps:SubscriptionKey"] ?? string.Empty;
    var useMockData = bool.Parse(configuration["AzureMaps:UseMockData"] ?? "true");
    return new AzureMapsTrafficClient(httpClient, subscriptionKey, useMockData);
});

// Add repositories
services.AddScoped<ITrafficRepository, TrafficRepository>();

// Add services
services.AddScoped<ITrafficTrackingService, TrafficTrackingService>();
services.AddScoped<ITrafficAnalyticsService, TrafficAnalyticsService>();

var serviceProvider = services.BuildServiceProvider();

// Ensure database is created
using (var scope = serviceProvider.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TrafficDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    Console.WriteLine("Database initialized.");
}

// Example usage
Console.WriteLine("TrafficTrack - Azure Traffic API Integration");
Console.WriteLine("===========================================\n");

try
{
    using var scope = serviceProvider.CreateScope();
    var trackingService = scope.ServiceProvider.GetRequiredService<ITrafficTrackingService>();
    var analyticsService = scope.ServiceProvider.GetRequiredService<ITrafficAnalyticsService>();

    // Define a bounding box (e.g., Seattle area)
    var seattleArea = new BoundingBox(47.7, -122.5, 47.5, -122.2);
    
    Console.WriteLine("Tracking traffic in specified area...");
    await trackingService.TrackTrafficAsync(seattleArea);
    Console.WriteLine("Traffic data collected and stored.\n");

    // Get analytics
    Console.WriteLine("=== Traffic Analytics ===\n");

    var incidents = await analyticsService.GetTrafficIncidentsAsync();
    Console.WriteLine($"Total Incidents: {incidents.Count()}");
    foreach (var incident in incidents.Take(5))
    {
        Console.WriteLine($"  - {incident.Type} at ({incident.Latitude:F4}, {incident.Longitude:F4}) - Severity: {incident.Severity}");
    }

    Console.WriteLine();

    var flows = await analyticsService.GetTrafficFlowsAsync();
    Console.WriteLine($"Total Flow Records: {flows.Count()}");
    foreach (var flow in flows.Take(5))
    {
        Console.WriteLine($"  - {flow.RoadName}: Current Speed {flow.CurrentSpeed:F1} km/h (Free Flow: {flow.FreeFlowSpeed:F1} km/h)");
    }

    Console.WriteLine();

    var incidentsByType = await analyticsService.GetIncidentCountByTypeAsync();
    Console.WriteLine("Incident Count by Type:");
    foreach (var kvp in incidentsByType)
    {
        Console.WriteLine($"  - {kvp.Key}: {kvp.Value}");
    }

    Console.WriteLine();

    var avgSpeedByArea = await analyticsService.GetAverageSpeedByAreaAsync();
    Console.WriteLine("Average Speed by Area:");
    foreach (var kvp in avgSpeedByArea)
    {
        Console.WriteLine($"  - {kvp.Key}: {kvp.Value:F2} km/h");
    }

    Console.WriteLine("\n=== Filtering Examples ===\n");

    // Filter by severity
    var criticalIncidents = await analyticsService.GetTrafficIncidentsAsync(severity: "Critical");
    Console.WriteLine($"Critical Incidents: {criticalIncidents.Count()}");

    // Filter by time range (last hour)
    var recentIncidents = await analyticsService.GetTrafficIncidentsAsync(from: DateTime.UtcNow.AddHours(-1));
    Console.WriteLine($"Incidents in Last Hour: {recentIncidents.Count()}");

    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
}
