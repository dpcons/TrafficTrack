using TrafficTrack.Core.Models;

namespace TrafficTrack.Core.Interfaces;

public interface ITrafficRepository
{
    Task<int> SaveTrafficIncidentsAsync(IEnumerable<TrafficIncident> incidents, CancellationToken cancellationToken = default);
    Task<int> SaveTrafficFlowsAsync(IEnumerable<TrafficFlow> flows, CancellationToken cancellationToken = default);
    Task<IEnumerable<TrafficIncident>> GetTrafficIncidentsAsync(DateTime? from = null, DateTime? to = null, string? area = null, string? severity = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<TrafficFlow>> GetTrafficFlowsAsync(DateTime? from = null, DateTime? to = null, string? area = null, CancellationToken cancellationToken = default);
}
