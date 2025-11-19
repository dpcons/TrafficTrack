using TrafficTrack.Core.Models;

namespace TrafficTrack.Core.Interfaces;

public interface ITrafficApiClient
{
    Task<IEnumerable<TrafficIncident>> GetTrafficIncidentsAsync(BoundingBox area, CancellationToken cancellationToken = default);
    Task<IEnumerable<TrafficFlow>> GetTrafficFlowAsync(BoundingBox area, CancellationToken cancellationToken = default);
}
