using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TrafficTrack.App.Models;

namespace TrafficTrack.App.Services;

public interface IApiService
{
    Task<(bool Success, LoginResponse? Data, string? Error)> LoginAsync(string username, string password);
    Task<(bool Success, LoginResponse? Data, string? Error)> RegisterAsync(string username, string password);
    Task<(bool Success, AreaTrafficResponse? Data, string? Error)> GetTrafficAsync(BoundingBox area);
    Task<(bool Success, AreaEventsResponse? Data, string? Error)> GetEventsAsync(BoundingBox area, string? eventType = null, string? severity = null);
    Task<(bool Success, List<SavedArea>? Data, string? Error)> GetSavedAreasAsync();
    Task<(bool Success, SavedArea? Data, string? Error)> SaveAreaAsync(SaveAreaRequest request);
    Task<(bool Success, string? Error)> DeleteAreaAsync(int id);
    void SetToken(string? token);
    bool IsAuthenticated { get; }
    string? CurrentUsername { get; }
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private string? _token;
    private string? _username;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);
    public string? CurrentUsername => _username;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public void SetToken(string? token)
    {
        _token = token;
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            _username = null;
        }
    }

    public async Task<(bool Success, LoginResponse? Data, string? Error)> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", 
                new { Username = username, Password = password });

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
                if (data != null)
                {
                    SetToken(data.Token);
                    _username = data.Username;
                }
                return (true, data, null);
            }

            var error = await TryReadError(response);
            return (false, null, error);
        }
        catch (Exception ex)
        {
            return (false, null, $"Errore di connessione: {ex.Message}");
        }
    }

    public async Task<(bool Success, LoginResponse? Data, string? Error)> RegisterAsync(string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register",
                new { Username = username, Password = password });

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
                if (data != null)
                {
                    SetToken(data.Token);
                    _username = data.Username;
                }
                return (true, data, null);
            }

            var error = await TryReadError(response);
            return (false, null, error);
        }
        catch (Exception ex)
        {
            return (false, null, $"Errore di connessione: {ex.Message}");
        }
    }

    public async Task<(bool Success, AreaTrafficResponse? Data, string? Error)> GetTrafficAsync(BoundingBox area)
    {
        try
        {
            var url = $"api/traffic?lat1={area.Lat1}&lon1={area.Lon1}&lat2={area.Lat2}&lon2={area.Lon2}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<AreaTrafficResponse>(_jsonOptions);
                return (true, data, null);
            }

            var error = await TryReadError(response);
            return (false, null, error);
        }
        catch (Exception ex)
        {
            return (false, null, $"Errore di connessione: {ex.Message}");
        }
    }

    public async Task<(bool Success, AreaEventsResponse? Data, string? Error)> GetEventsAsync(
        BoundingBox area, string? eventType = null, string? severity = null)
    {
        try
        {
            var url = $"api/events?lat1={area.Lat1}&lon1={area.Lon1}&lat2={area.Lat2}&lon2={area.Lon2}";
            if (!string.IsNullOrEmpty(eventType)) url += $"&eventType={eventType}";
            if (!string.IsNullOrEmpty(severity)) url += $"&severity={severity}";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<AreaEventsResponse>(_jsonOptions);
                return (true, data, null);
            }

            var error = await TryReadError(response);
            return (false, null, error);
        }
        catch (Exception ex)
        {
            return (false, null, $"Errore di connessione: {ex.Message}");
        }
    }

    public async Task<(bool Success, List<SavedArea>? Data, string? Error)> GetSavedAreasAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/savedareas");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<SavedArea>>(_jsonOptions);
                return (true, data, null);
            }

            var error = await TryReadError(response);
            return (false, null, error);
        }
        catch (Exception ex)
        {
            return (false, null, $"Errore di connessione: {ex.Message}");
        }
    }

    public async Task<(bool Success, SavedArea? Data, string? Error)> SaveAreaAsync(SaveAreaRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/savedareas", request);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<SavedArea>(_jsonOptions);
                return (true, data, null);
            }

            var error = await TryReadError(response);
            return (false, null, error);
        }
        catch (Exception ex)
        {
            return (false, null, $"Errore di connessione: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> DeleteAreaAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/savedareas/{id}");

            if (response.IsSuccessStatusCode)
                return (true, null);

            var error = await TryReadError(response);
            return (false, error);
        }
        catch (Exception ex)
        {
            return (false, $"Errore di connessione: {ex.Message}");
        }
    }

    private async Task<string> TryReadError(HttpResponseMessage response)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>(_jsonOptions);
            return error?.Message ?? $"Errore {(int)response.StatusCode}";
        }
        catch
        {
            return $"Errore {(int)response.StatusCode}: {response.ReasonPhrase}";
        }
    }
}
