# .NET 10 Upgrade Report

## Summary

L'upgrade della soluzione TrafficTrack a .NET 10 è stato completato con successo. 7 progetti su 8 sono stati aggiornati a .NET 10. Il progetto TrafficTrack.App (Uno Platform) è rimasto su .NET 9 poiché Uno Platform non supporta ancora .NET 10 in versione stabile.

## Project target framework modifications

| Project name                    | Old Target Framework          | New Target Framework           | Commits        |
|:--------------------------------|:-----------------------------:|:------------------------------:|:---------------|
| TrafficTrack.Core               | net8.0                        | net10.0                        | auto-upgrade   |
| TrafficTrack.Infrastructure     | net8.0                        | net10.0                        | auto-upgrade   |
| TrafficTrack.Services           | net8.0                        | net10.0                        | auto-upgrade   |
| TrafficTrack.App                | net9.0-windows10.0.22621      | net9.0-windows10.0.22621       | 3052205b       |
| TrafficTrack.Api                | net8.0                        | net10.0                        | edfb61ce       |
| TrafficTrack.Tests              | net8.0                        | net10.0                        | auto-upgrade   |
| TrafficTrack.Console            | net8.0                        | net10.0                        | auto-upgrade   |

## NuGet Packages

| Package Name                                           | Old Version     | New Version | Projects Affected                           |
|:-------------------------------------------------------|:---------------:|:-----------:|:--------------------------------------------|
| Microsoft.AspNetCore.Authentication.JwtBearer          | 8.0.11          | 10.0.1      | TrafficTrack.Api                            |
| Microsoft.EntityFrameworkCore.Design                   | 8.0.11          | 10.0.1      | TrafficTrack.Infrastructure                 |
| Microsoft.EntityFrameworkCore.Sqlite                   | 8.0.11          | 10.0.1      | TrafficTrack.Infrastructure, Api, Console   |
| Microsoft.Extensions.Configuration.Json                | 8.0.1           | 10.0.1      | TrafficTrack.Console                        |
| Microsoft.Extensions.DependencyInjection               | 8.0.1           | 10.0.1      | TrafficTrack.Console                        |
| Microsoft.Extensions.DependencyInjection.Abstractions  | 8.0.2           | 10.0.1      | TrafficTrack.Services                       |
| Microsoft.Extensions.Http                              | 8.0.1           | 10.0.1      | TrafficTrack.Infrastructure, Console        |
| Microsoft.OpenApi                                      | (implicit)      | 3.0.2       | TrafficTrack.Api (new dependency)           |
| Swashbuckle.AspNetCore                                 | 6.6.2           | 10.0.1      | TrafficTrack.Api                            |

## All commits

| Commit ID  | Description                                                                                                    |
|:-----------|:---------------------------------------------------------------------------------------------------------------|
| c2114438   | Upgrade plan commit                                                                                            |
| edfb61ce   | Store final changes for step 'Upgrade TrafficTrack.Api'                                                        |
| 3052205b   | TrafficTrack.App mantenuto su .NET 9 - Uno Platform non supporta ancora .NET 10                                |

## Code changes

### TrafficTrack.Api

- Aggiornato `Program.cs` per supportare le nuove API di OpenAPI 3.x:
  - Cambiato namespace da `Microsoft.OpenApi.Models` a `Microsoft.OpenApi`
  - Aggiornata la configurazione di `AddSecurityRequirement` per usare la nuova sintassi con lambda
  - Aggiunto pacchetto `Microsoft.OpenApi` 3.0.2 per i tipi OpenAPI
  - Aggiornato `Swashbuckle.AspNetCore` da 6.6.2 a 10.0.1

### TrafficTrack.App (Uno Platform)

- Il progetto è rimasto su .NET 9 poiché Uno Platform non ha ancora rilasciato una versione stabile compatibile con .NET 10
- I pacchetti Uno.WinUI e Uno.Toolkit sono rimasti alle versioni stabili esistenti

### global.json

- Aggiornata la versione SDK da 8.0.100 a 10.0.100
- Abilitato `allowPrerelease: true` per supportare .NET 10 Preview

## Test Results

| Project               | Total | Passed | Failed | Skipped |
|:----------------------|:-----:|:------:|:------:|:-------:|
| TrafficTrack.Tests    | 5     | 5      | 0      | 0       |

## Next steps

- Monitorare i rilasci di Uno Platform per una versione stabile compatibile con .NET 10 e aggiornare TrafficTrack.App quando disponibile
- Verificare eventuali breaking changes nelle API utilizzate dopo un periodo di testing in ambiente di sviluppo
- Considerare l'aggiornamento di `System.IdentityModel.Tokens.Jwt` a una versione più recente se disponibile

