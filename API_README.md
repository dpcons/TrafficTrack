# TrafficTrack - Sistema di Monitoraggio Traffico

## Architettura

```
???????????????????????????????????????????????????????????????????
?                    TrafficTrack Solution                        ?
???????????????????????????????????????????????????????????????????
?                                                                  ?
?  ????????????????????    ????????????????????                   ?
?  ?  TrafficTrack.App?????? TrafficTrack.Api ?                   ?
?  ?   (UNO Platform) ?    ?    (ASP.NET)     ?                   ?
?  ?                  ?    ?                  ?                   ?
?  ?  - Windows       ?    ?  - REST API      ?                   ?
?  ?  - Android       ?    ?  - JWT Auth      ?                   ?
?  ?  - iOS           ?    ?  - Swagger       ?                   ?
?  ?  - macOS         ?    ?  - SQLite DB     ?                   ?
?  ?  - WebAssembly   ?    ?                  ?                   ?
?  ?  - Linux         ?    ?                  ?                   ?
?  ????????????????????    ????????????????????                   ?
?                                   ?                              ?
?                          ????????????????????                   ?
?                          ?   Azure Maps     ?                   ?
?                          ?   Traffic API    ?                   ?
?                          ????????????????????                   ?
???????????????????????????????????????????????????????????????????
```

## TrafficTrack.Api - API RESTful

### Avvio

```bash
cd TrafficTrack.Api
dotnet run --urls "https://localhost:7001"
```

### Swagger UI

Disponibile su: `https://localhost:7001/swagger`

### Utente Demo

- **Username:** `demo`
- **Password:** `demo123`

### Endpoints

#### Autenticazione

| Metodo | Endpoint | Descrizione |
|--------|----------|-------------|
| POST | `/api/auth/login` | Login e ottieni token JWT |
| POST | `/api/auth/register` | Registra nuovo utente |
| GET | `/api/auth/verify` | Verifica validità token |

#### Traffico

| Metodo | Endpoint | Descrizione |
|--------|----------|-------------|
| GET | `/api/traffic?lat1=&lon1=&lat2=&lon2=` | Dati traffico per area |
| POST | `/api/traffic/refresh?...` | Aggiorna dati traffico |

#### Eventi

| Metodo | Endpoint | Descrizione |
|--------|----------|-------------|
| GET | `/api/events?lat1=&lon1=&lat2=&lon2=` | Eventi in area |
| GET | `/api/events/recent?hours=24` | Eventi recenti |
| GET | `/api/events/types` | Tipi di evento disponibili |
| GET | `/api/events/severities` | Livelli di severità |

**Filtri opzionali per /api/events:**
- `eventType` - Tipo evento (Accident, Construction, etc.)
- `severity` - Severità (Low, Moderate, Major, Critical)
- `fromDate` - Data inizio (ISO 8601)
- `toDate` - Data fine (ISO 8601)

#### Aree Salvate

| Metodo | Endpoint | Descrizione |
|--------|----------|-------------|
| GET | `/api/savedareas` | Lista aree salvate |
| GET | `/api/savedareas/{id}` | Dettaglio area |
| POST | `/api/savedareas` | Salva nuova area |
| PUT | `/api/savedareas/{id}` | Modifica area |
| DELETE | `/api/savedareas/{id}` | Elimina area |

### Esempio Chiamate

```bash
# Login
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"demo","password":"demo123"}'

# Ottieni traffico (con token)
curl -X GET "https://localhost:7001/api/traffic?lat1=45.19&lon1=9.15&lat2=45.18&lon2=9.19" \
  -H "Authorization: Bearer {token}"

# Ottieni solo incidenti
curl -X GET "https://localhost:7001/api/events?lat1=45.19&lon1=9.15&lat2=45.18&lon2=9.19&eventType=Accident" \
  -H "Authorization: Bearer {token}"
```

### Tipi di Evento

| Codice | Italiano |
|--------|----------|
| Accident | Incidente |
| Construction | Lavori Stradali |
| Roadblock | Blocco Stradale |
| TrafficJam | Ingorgo |
| RoadHazard | Pericolo Stradale |
| Event | Manifestazione |
| Weather | Condizioni Meteo |

### Livelli Severità

| Codice | Italiano |
|--------|----------|
| Low | Bassa |
| Moderate | Moderata |
| Major | Alta |
| Critical | Critica |

### Configurazione Azure Maps

Per usare dati reali da Azure Maps, modifica `appsettings.json`:

```json
{
  "AzureMaps": {
    "SubscriptionKey": "YOUR_AZURE_MAPS_KEY",
    "UseMockData": false
  }
}
```

---

## TrafficTrack.App - Applicazione Multipiattaforma

### Piattaforme Supportate

- ? Windows (WinUI 3)
- ? Android
- ? iOS
- ? macOS (Catalyst)
- ? WebAssembly
- ? Linux (Desktop)

### Funzionalità

1. **Autenticazione**
   - Login/Registrazione
   - Token JWT sicuro

2. **Selezione Area**
   - Input coordinate manuale
   - Aree salvate

3. **Visualizzazione Traffico**
   - Velocità media
   - Livello congestione
   - Dettagli per strada

4. **Eventi Stradali**
   - Lista eventi in area
   - Filtri per tipo/severità
   - Dettagli evento

5. **Aree Preferite**
   - Salvataggio rapido
   - Accesso veloce

### Configurazione

Modifica `ApiBaseUrl` in `App.xaml.cs`:

```csharp
private const string ApiBaseUrl = "https://your-api-server.com";
```

### Build ed Esecuzione

```bash
# Windows
cd TrafficTrack.App
dotnet build -t:Run -f net8.0-windows10.0.19041

# Android (richiede emulatore/device)
dotnet build -t:Run -f net8.0-android

# WebAssembly
dotnet build -f net8.0-browserwasm
```

---

## Struttura Progetto

```
TrafficTrack/
??? TrafficTrack.Api/           # API REST
?   ??? Controllers/            # Endpoints
?   ??? Data/                   # EF Core DbContext
?   ??? Models/                 # DTO
?   ??? Services/               # Business Logic
?
??? TrafficTrack.App/           # App UNO Platform
?   ??? Models/                 # Client DTO
?   ??? Services/               # API Client
?   ??? ViewModels/             # MVVM
?   ??? MainPage.xaml           # UI
?
??? TrafficTrack.Core/          # Modelli condivisi (legacy)
??? TrafficTrack.Infrastructure/# Accesso dati (legacy)
??? TrafficTrack.Services/      # Servizi (legacy)
??? TrafficTrack.Web/           # Blazor Web (legacy)
```

---

## Test API Verificati ?

```
? GET  /health                 ? {"status":"Healthy"}
? POST /api/auth/login         ? Token JWT
? POST /api/auth/register      ? Nuovo utente + Token
? GET  /api/traffic            ? 15 rilevazioni traffico
? GET  /api/events             ? 8 eventi stradali
? GET  /api/events/types       ? 7 tipi evento
? GET  /api/savedareas         ? Lista aree utente
? POST /api/savedareas         ? Salvataggio area
```
