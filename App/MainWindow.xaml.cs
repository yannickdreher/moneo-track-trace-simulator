using App.Enums;
using App.Pages;
using App.Services;
using App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.UI;

namespace App
{
    public sealed partial class MainWindow : Window
    {
        private readonly MqttService _mqttService;
        private readonly NavigationService _navigationService;
        private readonly LogService _logger;
        private readonly MqttViewModel _mqttViewModel;

        public MqttViewModel MqttViewModel => _mqttViewModel;

        public MainWindow(
            MqttService mqttService, 
            NavigationService navigationService, 
            MqttViewModel mqttViewModel,
            LogService logService)
        {
            InitializeComponent();
            
            _mqttService = mqttService;
            _navigationService = navigationService;
            _logger = logService;
            _mqttViewModel = mqttViewModel;
            
            LogListView.ItemsSource = _logger.LogEntries;
            
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            
            SetWindowSize(900, 1000);
            SetVersion();
            
            _navigationService.SetFrame(ContentFrame);
            _navigationService.NavigateTo<SimulationPage>();
            _logger.SetDispatcherQueue(DispatcherQueue);

            MainNavigationView.SelectedItem = MainNavigationView.MenuItems[0];

            _ = ConnectToBrokerAsync();
        }

        private void SetWindowSize(int width, int height)
        {
            var appWindow = AppWindow;
            appWindow?.Resize(new SizeInt32(width, height));
        }

        private void SetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            VersionText.Text = $"Version {version?.Major}.{version?.Minor}.{version?.Build}";
        }

        private async Task ConnectToBrokerAsync()
        {
            try
            {
                await _mqttService.ConnectAsync();
            }
            catch (Exception)
            {
                // Error is already logged by MqttService via LogService
            }
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                var tag = item.Tag?.ToString();
                switch (tag)
                {
                    case "simulation":
                        _navigationService.NavigateTo<SimulationPage>();
                        break;
                    case "settings":
                        _navigationService.NavigateTo<SettingsPage>();
                        break;
                }
            }
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            _logger.Clear();
        }

#pragma warning disable CA1822 // Mark members as static
        public Brush StatusToBrush(ConnectionStatus status)
#pragma warning restore CA1822 // Mark members as static
        {
            return status switch
            {
                ConnectionStatus.Connected => new SolidColorBrush(Color.FromArgb(255, 45, 125, 45)),
                ConnectionStatus.Connecting => new SolidColorBrush(Color.FromArgb(255, 255, 185, 0)),
                ConnectionStatus.Disconnected => new SolidColorBrush(Color.FromArgb(255, 196, 43, 28)),
                _ => new SolidColorBrush(Color.FromArgb(255, 128, 128, 128))
            };
        }
    }
}
