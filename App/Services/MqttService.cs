using App.Enums;
using App.Settings;
using MQTTnet;
using MQTTnet.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace App.Services
{
    public partial class MqttService : IDisposable
    {
        private readonly SettingsService<MqttSettings> _settingsService;
        private readonly IMqttClient _client;
        private readonly LogService _logger;
        private readonly MqttClientFactory _mqttFactory = new();
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly CancellationTokenSource _cts = new();
        private MqttSettings _settings;

        public event Action<ConnectionStatus>? StatusChanged;
        public ConnectionStatus CurrentStatus { get; private set; } = ConnectionStatus.Disconnected;

        public MqttService(SettingsService<MqttSettings> settingsService, LogService logService)
        {
            _settingsService = settingsService;
            _logger = logService;
            _settings = _settingsService.Value;

            _client = _mqttFactory.CreateMqttClient();

            // Handle connection events
            _client.DisconnectedAsync += OnDisconnectedAsync;
            _client.ConnectingAsync += OnConnectingAsync;
            _client.ConnectedAsync += OnConnectedAsync;

            // Listen for settings changes
            _settingsService.SettingsChanged += OnSettingsChangedAsync;
        }

        private async void OnSettingsChangedAsync(MqttSettings options)
        {
            _logger.LogInfo("[MQTT] Applying settings changes...");

            try
            {
                await DisconnectAsync();
                _settings = options;
                await ConnectAsync();
            }
            catch
            {
                // Ignore errors during settings change
            }
        }

        private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            UpdateStatus(ConnectionStatus.Disconnected);
            _logger.LogInfo($"[MQTT] Disconnected from {_settings.Host}:{_settings.Port}");
        }

        private async Task OnConnectingAsync(MqttClientConnectingEventArgs e)
        {
            UpdateStatus(ConnectionStatus.Connecting);
            _logger.LogInfo($"[MQTT] Connecting to {_settings.Host}:{_settings.Port}");
        }

        private async Task OnConnectedAsync(MqttClientConnectedEventArgs e)
        {
            UpdateStatus(ConnectionStatus.Connected);
            _logger.LogInfo($"[MQTT] Connected to {_settings.Host}:{_settings.Port}");
        }

        private void UpdateStatus(ConnectionStatus status)
        {
            if (CurrentStatus != status)
            {
                CurrentStatus = status;
                StatusChanged?.Invoke(status);
            }
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken = cancellationToken == default ? _cts.Token : cancellationToken;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (_client.IsConnected)
                {
                    return;
                }

                UpdateStatus(ConnectionStatus.Connecting);

                var clientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(_settings.Host, _settings.Port)
                    .WithCleanSession()
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(_settings.KeepAlivePeriodSeconds))
                    .WithTimeout(TimeSpan.FromSeconds(_settings.TimeoutSeconds));

                var tlsOptions = new MqttClientTlsOptionsBuilder()
                    .UseTls(_settings.UseTls)
                    .WithAllowUntrustedCertificates(true)
                    .Build();

                clientOptions.WithTlsOptions(tlsOptions);

                if (!string.IsNullOrEmpty(_settings.Username))
                {
                    clientOptions.WithCredentials(_settings.Username, _settings.Password);
                }

                var options = clientOptions.Build();
                await _client.ConnectAsync(options, cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken = cancellationToken == default ? _cts.Token : cancellationToken;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await _lock.WaitAsync(cancellationToken);

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await _client.DisconnectAsync(new MqttClientDisconnectOptions
                {
                    Reason = MqttClientDisconnectOptionsReason.NormalDisconnection
                }, cts.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[MQTT] Error during disconnect: {ex.Message}");
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task PublishAsync(string topic, string payload, CancellationToken cancellationToken = default)
        {
            cancellationToken = cancellationToken == default ? _cts.Token : cancellationToken;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (!_client.IsConnected)
            {
                await ConnectAsync(cancellationToken);
            }

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(false)
                .Build();

            await _client.PublishAsync(message, cancellationToken);
        }

        public void Dispose()
        {
            _cts.Cancel();

            // TODO: Not sure if we need to do this here
            //try
            //{
            //    DisconnectAsync(CancellationToken.None).GetAwaiter().GetResult();
            //}
            //catch (Exception ex)
            //{
            //    _logger.AddLog($"[MQTT] Error during disposal disconnect: {ex.Message}");
            //}

            _lock.Dispose();
            _client.Dispose();
            _cts.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}