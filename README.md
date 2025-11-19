# TrafficTrack

Test and implementation around Azure Traffic API to create a system that can track the traffic in a specified area and store it.

## Project Structure

The solution is organized as follows:

```
TrafficTrack/
├── src/
│   ├── TrafficTrack.CLI/          # Console application entry point
│   │   ├── Program.cs             # Main application logic
│   │   └── TrafficTrack.CLI.csproj
│   └── TrafficTrack.Core/         # Core business logic library
│       ├── TrafficData.cs         # Traffic data model
│       ├── ITrafficService.cs     # Interface for traffic API service
│       ├── ITrafficRepository.cs  # Interface for data storage
│       └── TrafficTrack.Core.csproj
└── tests/
    └── TrafficTrack.Tests/        # Unit tests
        ├── TrafficDataTests.cs    # Tests for traffic data model
        └── TrafficTrack.Tests.csproj
```

## Prerequisites

- .NET 9.0 SDK or later

## Building the Project

To build the entire solution:

```bash
dotnet build
```

## Running Tests

To run all tests:

```bash
dotnet test
```

## Running the Application

To run the console application:

```bash
dotnet run --project src/TrafficTrack.CLI/TrafficTrack.CLI.csproj
```

## Development

The project follows a clean architecture approach:

- **TrafficTrack.CLI**: Entry point for the application
- **TrafficTrack.Core**: Contains domain models, interfaces, and business logic
- **TrafficTrack.Tests**: Contains unit tests for the core library

### Next Steps

- Implement Azure Traffic API integration
- Implement data storage functionality
- Add configuration management
- Add logging and error handling
