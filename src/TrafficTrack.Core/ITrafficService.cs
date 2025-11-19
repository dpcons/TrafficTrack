namespace TrafficTrack.Core;

/// <summary>
/// Interface for traffic data service
/// </summary>
public interface ITrafficService
{
    /// <summary>
    /// Gets traffic data for a specified location
    /// </summary>
    /// <param name="location">The location to get traffic data for</param>
    /// <returns>Traffic data for the specified location</returns>
    Task<TrafficData?> GetTrafficDataAsync(string location);
}
