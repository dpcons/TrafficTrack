using TrafficTrack.Core.Interfaces;
using TrafficTrack.Core.Models;

namespace TrafficTrack.Services.Implementation;

public class TrafficAnalyticsService : ITrafficAnalyticsService
{
    private readonly ITrafficRepository _repository;

    public TrafficAnalyticsService(ITrafficRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TrafficIncident>> GetTrafficIncidentsAsync(DateTime? from = null, DateTime? to = null, string? area = null, string? severity = null, CancellationToken cancellationToken = default)
    {
        return await _repository.GetTrafficIncidentsAsync(from, to, area, severity, cancellationToken);
    }

    public async Task<IEnumerable<TrafficFlow>> GetTrafficFlowsAsync(DateTime? from = null, DateTime? to = null, string? area = null, CancellationToken cancellationToken = default)
    {
        return await _repository.GetTrafficFlowsAsync(from, to, area, cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetIncidentCountByTypeAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var incidents = await _repository.GetTrafficIncidentsAsync(from, to, null, null, cancellationToken);
        return incidents
            .GroupBy(i => i.Type)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, double>> GetAverageSpeedByAreaAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var flows = await _repository.GetTrafficFlowsAsync(from, to, null, cancellationToken);
        return flows
            .GroupBy(f => f.Area)
            .ToDictionary(g => g.Key, g => g.Average(f => f.CurrentSpeed));
    }
}
