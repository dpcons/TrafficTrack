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
        // Fetch traffic incidents
        var incidents = await _apiClient.GetTrafficIncidentsAsync(area, cancellationToken);
        if (incidents.Any())
        {
            await _repository.SaveTrafficIncidentsAsync(incidents, cancellationToken);
        }

        // Fetch traffic flow
        var flows = await _apiClient.GetTrafficFlowAsync(area, cancellationToken);
        if (flows.Any())
        {
            await _repository.SaveTrafficFlowsAsync(flows, cancellationToken);
        }
    }
}
