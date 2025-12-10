using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrafficTrack.App.Models;
using TrafficTrack.App.Services;

namespace TrafficTrack.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isAuthenticated;

    [ObservableProperty]
    private string _currentUsername = string.Empty;

    // Coordinate area
    [ObservableProperty]
    private string _lat1 = "45.19819";

    [ObservableProperty]
    private string _lon1 = "9.159349";

    [ObservableProperty]
    private string _lat2 = "45.187467";

    [ObservableProperty]
    private string _lon2 = "9.191268";

    // Dati traffico
    [ObservableProperty]
    private AreaTrafficResponse? _trafficData;

    [ObservableProperty]
    private AreaEventsResponse? _eventsData;

    // Aree salvate
    [ObservableProperty]
    private List<SavedArea> _savedAreas = [];

    // Filtri eventi
    [ObservableProperty]
    private string? _selectedEventType;

    [ObservableProperty]
    private string? _selectedSeverity;

    public List<string> EventTypeOptions => EventTypes.DisplayNames.Keys.ToList();
    public List<string> SeverityOptions => SeverityLevels.DisplayNames.Keys.ToList();

    public MainViewModel(IApiService apiService)
    {
        _apiService = apiService;
        UpdateAuthState();
    }

    private void UpdateAuthState()
    {
        IsAuthenticated = _apiService.IsAuthenticated;
        CurrentUsername = _apiService.CurrentUsername ?? string.Empty;
    }

    public void RefreshAuthState()
    {
        UpdateAuthState();
    }

    [RelayCommand]
    private async Task LoadTrafficAsync()
    {
        if (!ValidateCoordinates(out var box))
            return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        var (success, data, error) = await _apiService.GetTrafficAsync(box);

        if (success)
        {
            TrafficData = data;
        }
        else
        {
            ErrorMessage = error ?? "Errore sconosciuto";
        }

        IsLoading = false;
    }

    [RelayCommand]
    private async Task LoadEventsAsync()
    {
        if (!ValidateCoordinates(out var box))
            return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        var (success, data, error) = await _apiService.GetEventsAsync(box, SelectedEventType, SelectedSeverity);

        if (success)
        {
            EventsData = data;
        }
        else
        {
            ErrorMessage = error ?? "Errore sconosciuto";
        }

        IsLoading = false;
    }

    [RelayCommand]
    private async Task LoadAllDataAsync()
    {
        await Task.WhenAll(LoadTrafficAsync(), LoadEventsAsync());
    }

    [RelayCommand]
    private async Task LoadSavedAreasAsync()
    {
        IsLoading = true;
        var (success, data, error) = await _apiService.GetSavedAreasAsync();

        if (success && data != null)
        {
            SavedAreas = data;
        }
        else
        {
            ErrorMessage = error ?? "Errore nel caricamento aree salvate";
        }

        IsLoading = false;
    }

    [RelayCommand]
    private async Task SaveCurrentAreaAsync(string name)
    {
        if (!ValidateCoordinates(out var box))
            return;

        if (string.IsNullOrWhiteSpace(name))
        {
            ErrorMessage = "Inserisci un nome per l'area";
            return;
        }

        IsLoading = true;
        var request = new SaveAreaRequest(name, null, box.Lat1, box.Lon1, box.Lat2, box.Lon2);
        var (success, _, error) = await _apiService.SaveAreaAsync(request);

        if (success)
        {
            await LoadSavedAreasAsync();
        }
        else
        {
            ErrorMessage = error ?? "Errore nel salvataggio";
        }

        IsLoading = false;
    }

    [RelayCommand]
    private void SelectSavedArea(SavedArea area)
    {
        Lat1 = area.Lat1.ToString();
        Lon1 = area.Lon1.ToString();
        Lat2 = area.Lat2.ToString();
        Lon2 = area.Lon2.ToString();
    }

    [RelayCommand]
    private async Task DeleteSavedAreaAsync(int id)
    {
        IsLoading = true;
        var (success, error) = await _apiService.DeleteAreaAsync(id);

        if (success)
        {
            await LoadSavedAreasAsync();
        }
        else
        {
            ErrorMessage = error ?? "Errore nell'eliminazione";
        }

        IsLoading = false;
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SelectedEventType = null;
        SelectedSeverity = null;
    }

    [RelayCommand]
    private void Logout()
    {
        _apiService.SetToken(null);
        UpdateAuthState();
        TrafficData = null;
        EventsData = null;
        SavedAreas = [];
    }

    private bool ValidateCoordinates(out BoundingBox box)
    {
        box = default!;
        ErrorMessage = string.Empty;

        if (!double.TryParse(Lat1, out var lat1) ||
            !double.TryParse(Lon1, out var lon1) ||
            !double.TryParse(Lat2, out var lat2) ||
            !double.TryParse(Lon2, out var lon2))
        {
            ErrorMessage = "Coordinate non valide";
            return false;
        }

        box = new BoundingBox(lat1, lon1, lat2, lon2);

        if (!box.IsValid())
        {
            ErrorMessage = "Coordinate fuori range (Lat: -90/90, Lon: -180/180)";
            return false;
        }

        return true;
    }
}
