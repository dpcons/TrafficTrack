using Moq;
using TrafficTrack.Core.Interfaces;
using TrafficTrack.Core.Models;
using TrafficTrack.Services.Implementation;

namespace TrafficTrack.Tests.Services;

public class TrafficTrackingServiceTests
{
    private readonly Mock<ITrafficApiClient> _mockApiClient;
    private readonly Mock<ITrafficRepository> _mockRepository;
    private readonly TrafficTrackingService _service;

    public TrafficTrackingServiceTests()
    {
        _mockApiClient = new Mock<ITrafficApiClient>();
        _mockRepository = new Mock<ITrafficRepository>();
        _service = new TrafficTrackingService(_mockApiClient.Object, _mockRepository.Object);
    }

    [Fact]
    public async Task TrackTrafficAsync_ShouldSaveIncidentsAndFlows()
    {
        // Arrange
        var area = new BoundingBox(47.7, -122.5, 47.5, -122.2);
        var incidents = new List<TrafficIncident>
        {
            new TrafficIncident { IncidentId = "1", Type = "Accident", Latitude = 47.6, Longitude = -122.3 }
        };
        var flows = new List<TrafficFlow>
        {
            new TrafficFlow { CurrentSpeed = 50, FreeFlowSpeed = 60, Latitude = 47.6, Longitude = -122.3 }
        };

        _mockApiClient.Setup(x => x.GetTrafficIncidentsAsync(area, default))
            .ReturnsAsync(incidents);
        _mockApiClient.Setup(x => x.GetTrafficFlowAsync(area, default))
            .ReturnsAsync(flows);
        _mockRepository.Setup(x => x.SaveTrafficIncidentsAsync(incidents, default))
            .ReturnsAsync(1);
        _mockRepository.Setup(x => x.SaveTrafficFlowsAsync(flows, default))
            .ReturnsAsync(1);

        // Act
        await _service.TrackTrafficAsync(area);

        // Assert
        _mockApiClient.Verify(x => x.GetTrafficIncidentsAsync(area, default), Times.Once);
        _mockApiClient.Verify(x => x.GetTrafficFlowAsync(area, default), Times.Once);
        _mockRepository.Verify(x => x.SaveTrafficIncidentsAsync(incidents, default), Times.Once);
        _mockRepository.Verify(x => x.SaveTrafficFlowsAsync(flows, default), Times.Once);
    }

    [Fact]
    public async Task TrackTrafficAsync_WhenNoIncidents_ShouldNotSaveIncidents()
    {
        // Arrange
        var area = new BoundingBox(47.7, -122.5, 47.5, -122.2);
        var emptyIncidents = new List<TrafficIncident>();
        var flows = new List<TrafficFlow>
        {
            new TrafficFlow { CurrentSpeed = 50, FreeFlowSpeed = 60, Latitude = 47.6, Longitude = -122.3 }
        };

        _mockApiClient.Setup(x => x.GetTrafficIncidentsAsync(area, default))
            .ReturnsAsync(emptyIncidents);
        _mockApiClient.Setup(x => x.GetTrafficFlowAsync(area, default))
            .ReturnsAsync(flows);

        // Act
        await _service.TrackTrafficAsync(area);

        // Assert
        _mockRepository.Verify(x => x.SaveTrafficIncidentsAsync(It.IsAny<IEnumerable<TrafficIncident>>(), default), Times.Never);
        _mockRepository.Verify(x => x.SaveTrafficFlowsAsync(flows, default), Times.Once);
    }
}
