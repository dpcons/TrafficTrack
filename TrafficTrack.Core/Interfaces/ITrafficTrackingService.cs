using TrafficTrack.Core.Models;

namespace TrafficTrack.Core.Interfaces;

public interface ITrafficTrackingService
{
    Task TrackTrafficAsync(BoundingBox area, CancellationToken cancellationToken = default);
}
