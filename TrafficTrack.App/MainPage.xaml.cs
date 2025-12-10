using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TrafficTrack.App.Models;
using TrafficTrack.App.ViewModels;

namespace TrafficTrack.App;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }
    public LoginViewModel LoginVM { get; }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        LoginVM = App.GetService<LoginViewModel>();
        
        LoginVM.OnLoginSuccess += () =>
        {
            // Refresh the UI bindings - properties will notify automatically via ObservableProperty
            ViewModel.RefreshAuthState();
        };

        this.InitializeComponent();
    }

    private void OnUseSavedArea(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is SavedArea area)
        {
            ViewModel.SelectSavedAreaCommand.Execute(area);
        }
    }
}
