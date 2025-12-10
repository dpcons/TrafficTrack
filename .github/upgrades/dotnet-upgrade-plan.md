# .NET 10 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that a .NET 10 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10 upgrade.
3. Upgrade TrafficTrack.Core\TrafficTrack.Core.csproj
4. Upgrade TrafficTrack.Infrastructure\TrafficTrack.Infrastructure.csproj
5. Upgrade TrafficTrack.Services\TrafficTrack.Services.csproj
6. Upgrade TrafficTrack.App\TrafficTrack.App.csproj
7. Upgrade TrafficTrack.Api\TrafficTrack.Api.csproj
8. Upgrade TrafficTrack.Web\TrafficTrack.Web.csproj
9. Upgrade TrafficTrack.Tests\TrafficTrack.Tests.csproj
10. Upgrade TrafficTrack.Console\TrafficTrack.Console.csproj
11. Run unit tests to validate upgrade in the projects listed below:
    - TrafficTrack.Tests\TrafficTrack.Tests.csproj

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

Table below contains projects that do belong to the dependency graph for selected projects and should not be included in the upgrade.

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|
| (none)                                         |                             |

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                                           | Current Version | New Version | Description                    |
|:-------------------------------------------------------|:---------------:|:-----------:|:-------------------------------|
| Microsoft.AspNetCore.Authentication.JwtBearer          | 8.0.11          | 10.0.1      | Recommended for .NET 10        |
| Microsoft.AspNetCore.OpenApi                           | 8.0.22          | 10.0.1      | Recommended for .NET 10        |
| Microsoft.EntityFrameworkCore.Design                   | 8.0.11          | 10.0.1      | Recommended for .NET 10        |
| Microsoft.EntityFrameworkCore.Sqlite                   | 8.0.11          | 10.0.1      | Recommended for .NET 10        |
| Microsoft.Extensions.Configuration.Json                | 8.0.1           | 10.0.1      | Recommended for .NET 10        |
| Microsoft.Extensions.DependencyInjection               | 8.0.1           | 10.0.1      | Recommended for .NET 10        |
| Microsoft.Extensions.DependencyInjection.Abstractions  | 8.0.2           | 10.0.1      | Recommended for .NET 10        |
| Microsoft.Extensions.Http                              | 8.0.1;9.0.0     | 10.0.1      | Recommended for .NET 10        |
| Microsoft.Extensions.Logging.Console                   | 9.0.0           | 10.0.1      | Recommended for .NET 10        |

### Project upgrade details

This section contains details about each project upgrade and modifications that need to be done in the project.

#### TrafficTrack.Core modifications

Project properties changes:
- Target framework should be changed from `net8.0` to `net10.0`

#### TrafficTrack.Infrastructure modifications

Project properties changes:
- Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
- Microsoft.EntityFrameworkCore.Design should be updated from `8.0.11` to `10.0.1` (*recommended for .NET 10*)
- Microsoft.EntityFrameworkCore.Sqlite should be updated from `8.0.11` to `10.0.1` (*recommended for .NET 10*)
- Microsoft.Extensions.Http should be updated from `8.0.1` to `10.0.1` (*recommended for .NET 10*)

#### TrafficTrack.Services modifications

Project properties changes:
- Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
- Microsoft.Extensions.DependencyInjection.Abstractions should be updated from `8.0.2` to `10.0.1` (*recommended for .NET 10*)

#### TrafficTrack.App modifications

Project properties changes:
- Target framework should be changed from `net9.0-windows10.0.22621` to `net10.0-windows10.0.22621.0`

NuGet packages changes:
- Microsoft.Extensions.Http should be updated from `9.0.0` to `10.0.1` (*recommended for .NET 10*)
- Microsoft.Extensions.Logging.Console should be updated from `9.0.0` to `10.0.1` (*recommended for .NET 10*)

#### TrafficTrack.Api modifications

Project properties changes:
- Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
- Microsoft.AspNetCore.Authentication.JwtBearer should be updated from `8.0.11` to `10.0.1` (*recommended for .NET 10*)
- Microsoft.AspNetCore.OpenApi should be updated from `8.0.22` to `10.0.1` (*recommended for .NET 10*)
- Microsoft.EntityFrameworkCore.Sqlite should be updated from `8.0.11` to `10.0.1` (*recommended for .NET 10*)

#### TrafficTrack.Web modifications

Project properties changes:
- Target framework should be changed from `net8.0` to `net10.0`

#### TrafficTrack.Tests modifications

Project properties changes:
- Target framework should be changed from `net8.0` to `net10.0`

#### TrafficTrack.Console modifications

Project properties changes:
- Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
- Microsoft.EntityFrameworkCore.Sqlite should be updated from `8.0.11` to `10.0.1` (*recommended for .NET 10*)
- Microsoft.Extensions.Configuration.Json should be updated from `8.0.1` to `10.0.1` (*recommended for .NET 10*)
- Microsoft.Extensions.DependencyInjection should be updated from `8.0.1` to `10.0.1` (*recommended for .NET 10*)
- Microsoft.Extensions.Http should be updated from `8.0.1` to `10.0.1` (*recommended for .NET 10*)
