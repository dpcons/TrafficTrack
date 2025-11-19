using TrafficTrack.Core.Models;

namespace TrafficTrack.Core.Interfaces;

public interface ITrafficAnalyticsService
{
    Task<IEnumerable<TrafficIncident>> GetTrafficIncidentsAsync(DateTime? from = null, DateTime? to = null, string? area = null, string? severity = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<TrafficFlow>> GetTrafficFlowsAsync(DateTime? from = null, DateTime? to = null, string? area = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetIncidentCountByTypeAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, double>> GetAverageSpeedByAreaAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
}
