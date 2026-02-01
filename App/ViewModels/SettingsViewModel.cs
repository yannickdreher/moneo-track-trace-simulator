using App.Settings;
using App.Services;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace App.ViewModels
{
    public partial class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly SettingsService<MqttSettings> _settingsService;

        private string _host = string.Empty;
        public string Host
        {
            get => _host;
            set => SetProperty(ref _host, value);
        }

        private int _port;
        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        private string? _username;
        public string? Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string? _password;
        public string? Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _topic = string.Empty;
        public string Topic
        {
            get => _topic;
            set => SetProperty(ref _topic, value);
        }

        private bool _useTls;
        public bool UseTls
        {
            get => _useTls;
            set => SetProperty(ref _useTls, value);
        }

        private int _timeoutSeconds;
        public int TimeoutSeconds
        {
            get => _timeoutSeconds;
            set => SetProperty(ref _timeoutSeconds, value);
        }

        private int _keepAlivePeriodSeconds;
        public int KeepAlivePeriodSeconds
        {
            get => _keepAlivePeriodSeconds;
            set => SetProperty(ref _keepAlivePeriodSeconds, value);
        }

        private string? _statusTitle;
        public string? StatusTitle
        {
            get => _statusTitle;
            set => SetProperty(ref _statusTitle, value);
        }

        private string? _statusMessage;
        public string? StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private InfoBarSeverity _statusSeverity;
        public InfoBarSeverity StatusSeverity
        {
            get => _statusSeverity;
            set => SetProperty(ref _statusSeverity, value);
        }

        private bool _isStatusOpen;
        public bool IsStatusOpen
        {
            get => _isStatusOpen;
            set => SetProperty(ref _isStatusOpen, value);
        }

        public SettingsViewModel(SettingsService<MqttSettings> settingsService)
        {
            _settingsService = settingsService;
            LoadSettings();
        }

        private void LoadSettings()
        {
            var settings = _settingsService.Value;

            Host = settings.Host;
            Port = settings.Port;
            Username = settings.Username;
            Password = settings.Password;
            Topic = settings.Topic;
            UseTls = settings.UseTls;
            TimeoutSeconds = settings.TimeoutSeconds;
            KeepAlivePeriodSeconds = settings.KeepAlivePeriodSeconds;
        }

        private void ShowMessage(string title, string message, InfoBarSeverity severity)
        {
            StatusTitle = title;
            StatusMessage = message;
            StatusSeverity = severity;
            IsStatusOpen = true;
        }

        public async Task SaveAsync()
        {
            try
            {
                var settings = new MqttSettings
                {
                    Host = Host,
                    Port = Port,
                    Username = string.IsNullOrWhiteSpace(Username) ? null : Username,
                    Password = string.IsNullOrWhiteSpace(Password) ? null : Password,
                    Topic = Topic,
                    UseTls = UseTls,
                    TimeoutSeconds = TimeoutSeconds,
                    KeepAlivePeriodSeconds = KeepAlivePeriodSeconds
                };

                await _settingsService.SaveAsync(settings);

                ShowMessage("Success", "Settings saved and will be applied automatically.", InfoBarSeverity.Success);
            }
            catch (Exception ex)
            {
                ShowMessage("Error", $"Failed to save settings: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}