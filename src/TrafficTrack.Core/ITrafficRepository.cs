namespace TrafficTrack.Core;

/// <summary>
/// Interface for traffic data repository
/// </summary>
public interface ITrafficRepository
{
    /// <summary>
    /// Saves traffic data to storage
    /// </summary>
    /// <param name="data">The traffic data to save</param>
    /// <returns>True if save was successful, false otherwise</returns>
    Task<bool> SaveTrafficDataAsync(TrafficData data);
    
    /// <summary>
    /// Retrieves traffic data for a location within a time range
    /// </summary>
    /// <param name="location">The location to retrieve data for</param>
    /// <param name="startTime">Start of time range</param>
    /// <param name="endTime">End of time range</param>
    /// <returns>Collection of traffic data</returns>
    Task<IEnumerable<TrafficData>> GetTrafficDataAsync(string location, DateTime startTime, DateTime endTime);
}
