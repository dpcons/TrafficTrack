using TrafficTrack.Core.Models;

namespace TrafficTrack.Core.Interfaces;

public interface ITrafficRepository
{
    Task<int> SaveTrafficIncidentsAsync(IEnumerable<TrafficIncident> incidents, CancellationToken cancellationToken = default);
    Task<int> SaveTrafficFlowsAsync(IEnumerable<TrafficFlow> flows, CancellationToken cancellationToken = default);
    Task<IEnumerable<TrafficIncident>> GetTrafficIncidentsAsync(DateTime? from = null, DateTime? to = null, string? area = null, string? severity = null, int? monitoringSessionId = null,
        double? minLat = null, double? maxLat = null, double? minLon = null, double? maxLon = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<TrafficFlow>> GetTrafficFlowsAsync(DateTime? from = null, DateTime? to = null, string? area = null, int? monitoringSessionId = null,
        double? minLat = null, double? maxLat = null, double? minLon = null, double? maxLon = null, CancellationToken cancellationToken = default);
}
