using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using App.Services;
using App.Pages;
using App.Settings;
using App.ViewModels;
using System;

namespace App
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;
        private Window? _window;

        public App()
        {
            InitializeComponent();

            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Services
            services.AddSingleton<SettingsService<MqttSettings>>();
            services.AddSingleton<MqttService>();
            services.AddSingleton<StationService>();
            services.AddSingleton<LogService>();
            services.AddSingleton<NavigationService>();

            // Factories
            services.AddSingleton<StationViewModelFactory>();

            // ViewModels
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<MqttViewModel>();

            // Pages
            services.AddSingleton<SimulationPage>();
            services.AddSingleton<SettingsPage>();
            services.AddSingleton<MainWindow>();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = Services.GetRequiredService<MainWindow>();
            _window.Closed += OnWindowClosed;
            _window.Activate();
        }

        private void OnWindowClosed(object sender, WindowEventArgs args)
        {
            if (Services is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
