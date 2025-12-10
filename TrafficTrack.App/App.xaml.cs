using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using TrafficTrack.App.Services;
using TrafficTrack.App.ViewModels;

namespace TrafficTrack.App;

public partial class App : Application
{
    private static IServiceProvider? _serviceProvider;
    private Window? _window;

    // URL dell'API (configurabile)
    private const string ApiBaseUrl = "https://localhost:7001"; // Cambiare in produzione

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Configura servizi
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        _window = new Window();
        _window.Content = new MainPage();
        _window.Activate();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // HttpClient configurato
        services.AddHttpClient<IApiService, ApiService>(client =>
        {
            client.BaseAddress = new Uri(ApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<LoginViewModel>();
    }

    public static T GetService<T>() where T : notnull
    {
        if (_serviceProvider == null)
            throw new InvalidOperationException("Service provider not initialized");

        return _serviceProvider.GetRequiredService<T>();
    }
}
