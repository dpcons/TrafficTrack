# TrafficTrack

A .NET 8.0 application that integrates with Azure Maps Traffic API to track traffic in specified areas, store the data, and provide analytics with filtering capabilities.

## Features

- **Azure Maps Traffic API Integration**: Fetch real-time traffic incidents and flow data
- **Mock Data Support**: Built-in mock data generation for testing without API credentials
- **Data Storage**: SQLite database with Entity Framework Core for persistent storage
- **Traffic Incident Tracking**: Record accidents, construction, roadblocks, and traffic jams
- **Traffic Flow Tracking**: Monitor current speeds vs. free-flow speeds
- **Analytics with Filtering**: Query data by time range, area, severity, and type
- **Aggregated Statistics**: 
  - Incident count by type
  - Average speed by area
  - Time-based trends

## Project Structure

```
TrafficTrack/
├── TrafficTrack.Core/              # Domain models and interfaces
│   ├── Models/                     # Traffic data models
│   └── Interfaces/                 # Service contracts
├── TrafficTrack.Infrastructure/    # Data access and external APIs
│   ├── AzureMaps/                  # Azure Maps API client
│   └── Data/                       # EF Core DbContext and repositories
├── TrafficTrack.Services/          # Business logic
│   └── Implementation/             # Service implementations
├── TrafficTrack.Console/           # Console application demo
└── TrafficTrack.Tests/             # Unit tests
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- (Optional) Azure Maps subscription key for real API calls

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/dpcons/TrafficTrack.git
   cd TrafficTrack
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

### Configuration

Edit `TrafficTrack.Console/appsettings.json`:

```json
{
  "AzureMaps": {
    "SubscriptionKey": "your-subscription-key-here",
    "UseMockData": true  // Set to false to use real Azure Maps API
  },
  "Database": {
    "ConnectionString": "Data Source=traffictrack.db"
  }
}
```

**Note**: If `UseMockData` is set to `true`, the application will generate realistic mock data without requiring an Azure Maps subscription key.

### Running the Application

```bash
cd TrafficTrack.Console
dotnet run
```

The console application will:
1. Initialize the SQLite database
2. Track traffic in a sample area (Seattle by default)
3. Display collected traffic incidents and flow data
4. Show analytics including incident counts by type and average speeds by area
5. Demonstrate filtering capabilities

### Running Tests

```bash
dotnet test
```

All 5 unit tests should pass, covering:
- Traffic tracking service
- Analytics service
- Data filtering
- Aggregation functions

## Usage Example

```csharp
// Define a bounding box for the area you want to track
var area = new BoundingBox(
    minLatitude: 47.5, 
    minLongitude: -122.5, 
    maxLatitude: 47.7, 
    maxLongitude: -122.2
);

// Track traffic
await trackingService.TrackTrafficAsync(area);

// Get analytics
var incidents = await analyticsService.GetTrafficIncidentsAsync(
    from: DateTime.UtcNow.AddHours(-24),
    severity: "Critical"
);

var incidentsByType = await analyticsService.GetIncidentCountByTypeAsync(
    from: DateTime.UtcNow.AddDays(-7)
);

var avgSpeedByArea = await analyticsService.GetAverageSpeedByAreaAsync(
    from: DateTime.UtcNow.AddHours(-1)
);
```

## API Integration

### Azure Maps Traffic API

The system integrates with two Azure Maps Traffic API endpoints:

1. **Traffic Incident Details API**: Provides information about traffic incidents (accidents, construction, etc.)
2. **Traffic Flow Segment API**: Provides real-time traffic flow data (currently using mock data)

For more information about Azure Maps Traffic services, visit:
https://docs.microsoft.com/en-us/azure/azure-maps/traffic-coverage

## Database Schema

### TrafficIncidents Table
- Id (Primary Key)
- IncidentId (Unique identifier from API)
- Latitude, Longitude (Location)
- Type (Accident, Construction, etc.)
- Description
- Severity (Low, Medium, High, Critical)
- StartTime, EndTime
- RecordedAt (Indexed)
- RoadName
- Area (Indexed)

### TrafficFlows Table
- Id (Primary Key)
- Latitude, Longitude (Location)
- CurrentSpeed, FreeFlowSpeed
- CurrentTravelTime, FreeFlowTravelTime
- Confidence
- RoadName
- RecordedAt (Indexed)
- Area (Indexed)

## Analytics Features

The analytics service provides several methods for data analysis:

- **GetTrafficIncidentsAsync**: Filter incidents by time range, area, and severity
- **GetTrafficFlowsAsync**: Filter traffic flow data by time range and area
- **GetIncidentCountByTypeAsync**: Aggregate incident counts grouped by type
- **GetAverageSpeedByAreaAsync**: Calculate average speeds grouped by area

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details. 
