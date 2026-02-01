using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Dispatching;
using App.Enums;
using App.Services;

namespace App.ViewModels
{
    public partial class MqttViewModel : INotifyPropertyChanged
    {
        private readonly MqttService _mqttService;
        private readonly DispatcherQueue _dispatcherQueue;
        private ConnectionStatus _status;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ConnectionStatus Status
        {
            get => _status;
            private set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsConnected));
                    OnPropertyChanged(nameof(IsConnecting));
                    OnPropertyChanged(nameof(IsDisconnected));
                }
            }
        }

        public bool IsConnected => Status == ConnectionStatus.Connected;
        public bool IsConnecting => Status == ConnectionStatus.Connecting;
        public bool IsDisconnected => Status == ConnectionStatus.Disconnected;

        public MqttViewModel(MqttService mqttService)
        {
            _mqttService = mqttService;
            _status = _mqttService.CurrentStatus;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _mqttService.StatusChanged += OnStatusChanged;
        }

        private void OnStatusChanged(ConnectionStatus newStatus)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                Status = newStatus;
            });
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}