using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrafficTrack.App.Services;

namespace TrafficTrack.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    public event Action? OnLoginSuccess;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isRegisterMode;

    public LoginViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Inserisci username e password";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        var (success, _, error) = IsRegisterMode
            ? await _apiService.RegisterAsync(Username, Password)
            : await _apiService.LoginAsync(Username, Password);

        if (success)
        {
            OnLoginSuccess?.Invoke();
        }
        else
        {
            ErrorMessage = error ?? "Errore di autenticazione";
        }

        IsLoading = false;
    }

    [RelayCommand]
    private void ToggleMode()
    {
        IsRegisterMode = !IsRegisterMode;
        ErrorMessage = string.Empty;
    }

    public string ActionButtonText => IsRegisterMode ? "Registrati" : "Accedi";
    public string ToggleButtonText => IsRegisterMode ? "Hai già un account? Accedi" : "Non hai un account? Registrati";
}
