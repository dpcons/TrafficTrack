using FluentAssertions;
using Moq;
using TrafficTrack.Core.Interfaces;
using TrafficTrack.Core.Models;
using TrafficTrack.Services.Implementation;

namespace TrafficTrack.Tests.Services;

public class TrafficAnalyticsServiceTests
{
    private readonly Mock<ITrafficRepository> _mockRepository;
    private readonly TrafficAnalyticsService _service;

    public TrafficAnalyticsServiceTests()
    {
        _mockRepository = new Mock<ITrafficRepository>();
        _service = new TrafficAnalyticsService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetTrafficIncidentsAsync_ShouldReturnFilteredIncidents()
    {
        var incidents = new List<TrafficIncident>
        {
            new TrafficIncident { Type = "Accident", Severity = "High", Area = "Seattle" },
            new TrafficIncident { Type = "Construction", Severity = "Low", Area = "Seattle" }
        };

        _mockRepository.Setup(x => x.GetTrafficIncidentsAsync(
            It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
            "Seattle", "High", It.IsAny<int?>(),
            It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<CancellationToken>()
        )).ReturnsAsync(incidents.Where(i => i.Area == "Seattle" && i.Severity == "High"));

        var result = await _service.GetTrafficIncidentsAsync(area: "Seattle", severity: "High");
        result.Should().HaveCount(1);
        result.First().Type.Should().Be("Accident");
    }

    [Fact]
    public async Task GetIncidentCountByTypeAsync_ShouldGroupByType()
    {
        var incidents = new List<TrafficIncident>
        {
            new TrafficIncident { Type = "Accident" },
            new TrafficIncident { Type = "Accident" },
            new TrafficIncident { Type = "Construction" }
        };

        _mockRepository.Setup(x => x.GetTrafficIncidentsAsync(
            It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int?>(),
            It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<CancellationToken>()
        )).ReturnsAsync(incidents);

        var result = await _service.GetIncidentCountByTypeAsync();
        result.Should().HaveCount(2);
        result["Accident"].Should().Be(2);
        result["Construction"].Should().Be(1);
    }

    [Fact]
    public async Task GetAverageSpeedByAreaAsync_ShouldCalculateAverageSpeed()
    {
        var flows = new List<TrafficFlow>
        {
            new TrafficFlow { Area = "Seattle", CurrentSpeed = 50 },
            new TrafficFlow { Area = "Seattle", CurrentSpeed = 60 },
            new TrafficFlow { Area = "Bellevue", CurrentSpeed = 70 }
        };

        _mockRepository.Setup(x => x.GetTrafficFlowsAsync(
            It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
            It.IsAny<string?>(), It.IsAny<int?>(),
            It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<CancellationToken>()
        )).ReturnsAsync(flows);

        var result = await _service.GetAverageSpeedByAreaAsync();
        result.Should().HaveCount(2);
        result["Seattle"].Should().Be(55);
        result["Bellevue"].Should().Be(70);
    }
}
