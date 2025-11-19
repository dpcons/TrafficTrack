using TrafficTrack.Core;

namespace TrafficTrack.Tests;

public class TrafficDataTests
{
    [Fact]
    public void TrafficData_CanBeCreated()
    {
        // Arrange & Act
        var trafficData = new TrafficData
        {
            Location = "Test Location",
            Timestamp = DateTime.Now,
            Speed = 60.5,
            Conditions = "Clear"
        };

        // Assert
        Assert.NotNull(trafficData);
        Assert.Equal("Test Location", trafficData.Location);
        Assert.Equal(60.5, trafficData.Speed);
        Assert.Equal("Clear", trafficData.Conditions);
    }
}
