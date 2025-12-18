using TrafficTrack.Core.Interfaces;
using TrafficTrack.Core.Models;

namespace TrafficTrack.Services.Implementation;

public class TrafficTrackingService : ITrafficTrackingService
{
    private readonly ITrafficApiClient _apiClient;
    private readonly ITrafficRepository _repository;

    public TrafficTrackingService(ITrafficApiClient apiClient, ITrafficRepository repository)
    {
        _apiClient = apiClient;
        _repository = repository;
    }

    public async Task TrackTrafficAsync(BoundingBox area, CancellationToken cancellationToken = default)
    {
        await TrackTrafficAsync(area, null, cancellationToken);
    }

    public async Task TrackTrafficAsync(BoundingBox area, int? monitoringSessionId, CancellationToken cancellationToken = default)
    {
        // Fetch traffic incidents
        var incidents = await _apiClient.GetTrafficIncidentsAsync(area, cancellationToken);
        if (incidents.Any())
        {
            if (monitoringSessionId.HasValue)
            {
                foreach (var i in incidents) i.MonitoringSessionId = monitoringSessionId.Value;
            }
            await _repository.SaveTrafficIncidentsAsync(incidents, cancellationToken);
        }

        // Fetch traffic flow
        var flows = await _apiClient.GetTrafficFlowAsync(area, cancellationToken);
        if (flows.Any())
        {
            if (monitoringSessionId.HasValue)
            {
                foreach (var f in flows) f.MonitoringSessionId = monitoringSessionId.Value;
            }
            await _repository.SaveTrafficFlowsAsync(flows, cancellationToken);
        }
    }
}
