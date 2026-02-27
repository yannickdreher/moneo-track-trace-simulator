using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using App.Services;
using App.Pages;
using App.Settings;
using App.ViewModels;
using System;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace App
{
    public partial class App : Application
    {
        private const string REPO_URL = "https://github.com/yannickdreher/moneo-track-trace-simulator";
        public static IServiceProvider Services { get; private set; } = null!;
        private Window? _window;

        public App()
        {
            VelopackApp.Build().Run();
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

            await CheckForUpdatesAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            if (_window?.Content is not FrameworkElement root || root.XamlRoot is null)
            {
                return;
            }

            try
            {
                var source = new GithubSource(REPO_URL, null, false);
                var updateManager = new UpdateManager(source);
                var updateInfo = await updateManager.CheckForUpdatesAsync();

                if (updateInfo is null)
                {
                    return;
                }

                var dialog = new ContentDialog
                {
                    XamlRoot = root.XamlRoot,
                    Title = "Update available",
                    Content = "A new version is available. Do you want to update now?",
                    PrimaryButtonText = "Update",
                    CloseButtonText = "Later",
                    DefaultButton = ContentDialogButton.Primary
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await updateManager.DownloadUpdatesAsync(updateInfo);
                    updateManager.ApplyUpdatesAndRestart(updateInfo);
                }
            }
            catch (Exception)
            {
                // Intentionally ignore update errors to avoid blocking app startup.
            }
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
