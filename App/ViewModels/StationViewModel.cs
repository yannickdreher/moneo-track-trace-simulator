using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using App.Models;
using App.Services;

namespace App.ViewModels
{
    public partial class StationViewModel(
        StationModel model,
        StationService stationService,
        LogService logService) : INotifyPropertyChanged
    {
        private readonly StationService _stationService = stationService;
        private readonly LogService _logger = logService;
        private readonly StationModel _model = model;
        private Timer? _telemetryTimer;
        private bool _isTelemetryRunning;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Observable Collections for UI Binding
        public ObservableCollection<CarrierItem> Carriers { get; } = [];
        public ObservableCollection<TelemetryDataPoint> TelemetryDataPoints { get; } = [];
        public ObservableCollection<AttributeDataPoint> Attributes { get; } = [];

        // Properties
        public string Name => _model.Name;
        public string BaseTopic => _model.BaseTopic;

        private int _telemetryIntervalSeconds = model.TelemetryIntervalSeconds;
        public int TelemetryIntervalSeconds
        {
            get => _telemetryIntervalSeconds;
            set
            {
                if (SetProperty(ref _telemetryIntervalSeconds, value))
                {
                    _model.TelemetryIntervalSeconds = value;
                }
            }
        }

        public bool IsTelemetryRunning
        {
            get => _isTelemetryRunning;
            private set
            {
                if (SetProperty(ref _isTelemetryRunning, value))
                {
                    OnPropertyChanged(nameof(CanStartTelemetry));
                    OnPropertyChanged(nameof(CanStopTelemetry));
                }
            }
        }

        public bool CanStartTelemetry => !IsTelemetryRunning;
        public bool CanStopTelemetry => IsTelemetryRunning;

        public void InitializeDefaultCarriers()
        {
            Carriers.Add(new CarrierItem { Name = "CARRIER001", IsSelected = true });
            Carriers.Add(new CarrierItem { Name = "CARRIER002", IsSelected = true });
            Carriers.Add(new CarrierItem { Name = "CARRIER003", IsSelected = true });
        }

        public void AddCarrier(string carrierName)
        {
            if (!string.IsNullOrWhiteSpace(carrierName))
            {
                var exists = false;
                foreach (var carrier in Carriers)
                {
                    if (carrier.Name == carrierName)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    Carriers.Add(new CarrierItem { Name = carrierName, IsSelected = true });
                }
            }
        }

        public void RemoveCarrier(string carrierName)
        {
            for (int i = Carriers.Count - 1; i >= 0; i--)
            {
                if (Carriers[i].Name == carrierName)
                {
                    Carriers.RemoveAt(i);
                    break;
                }
            }
        }

        public void SelectAllCarriers()
        {
            foreach (var carrier in Carriers)
            {
                carrier.IsSelected = true;
            }
        }

        public void DeselectAllCarriers()
        {
            foreach (var carrier in Carriers)
            {
                carrier.IsSelected = false;
            }
        }

        public string[] GetSelectedCarriers()
        {
            var selected = new System.Collections.Generic.List<string>();
            foreach (var carrier in Carriers)
            {
                if (carrier.IsSelected)
                {
                    selected.Add(carrier.Name);
                }
            }
            return [.. selected];
        }

        public async Task SendEntryAsync()
        {
            var selectedCarriers = GetSelectedCarriers();
            if (selectedCarriers.Length == 0)
            {
                _logger.LogInfo("No carriers selected for entry");
                return;
            }

            try
            {
                await _stationService.PublishEntryAsync(selectedCarriers, BaseTopic);
                _logger.LogInfo($"[{Name}] Entry sent for {selectedCarriers.Length} carrier(s)");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{Name}] Failed to send entry: {ex.Message}");
            }
        }

        public async Task SendExitAsync()
        {
            var selectedCarriers = GetSelectedCarriers();
            if (selectedCarriers.Length == 0)
            {
                _logger.LogInfo("No carriers selected for exit");
                return;
            }

            try
            {
                await _stationService.PublishExitAsync(selectedCarriers, BaseTopic, Name);
                _logger.LogInfo($"[{Name}] Exit sent for {selectedCarriers.Length} carrier(s)");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{Name}] Failed to send exit: {ex.Message}");
            }
        }

        public void StartTelemetry()
        {
            if (IsTelemetryRunning) return;

            _telemetryTimer = new Timer(
                async _ => await SendTelemetryAsync(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(TelemetryIntervalSeconds));

            IsTelemetryRunning = true;
            _logger.LogInfo($"[{Name}] Telemetry started (interval: {TelemetryIntervalSeconds}s)");
        }

        public void StopTelemetry()
        {
            _telemetryTimer?.Dispose();
            _telemetryTimer = null;
            IsTelemetryRunning = false;
            _logger.LogInfo($"[{Name}] Telemetry stopped");
        }

        private async Task SendTelemetryAsync()
        {
            foreach (var dataPoint in TelemetryDataPoints)
            {
                double value = dataPoint.Value;
                if (dataPoint.UseRandom)
                {
                    var random = new Random();
                    value = dataPoint.MinValue + (random.NextDouble() * (dataPoint.MaxValue - dataPoint.MinValue));
                    value = Math.Round(value, 2);
                }

                try
                {
                    await _stationService.PublishTelemetryAsync(dataPoint, value, BaseTopic);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[{Name}] Failed to send telemetry for {dataPoint.Identifier}: {ex.Message}");
                }
            }
        }

        public async Task SendAttributesAsync()
        {
            foreach (var attr in Attributes)
            {
                if (!string.IsNullOrWhiteSpace(attr.Value))
                {
                    try
                    {
                        await _stationService.PublishAttributeAsync(attr, BaseTopic);
                    }
                    catch
                    {
                        _logger.LogError($"[{Name}] Failed to send attribute {attr.Identifier}");
                    }
                }
            }

            _logger.LogInfo($"[{Name}] Attributes sent");
        }

        public async Task SendStateAsync(string state)
        {
            try
            {
                await _stationService.PublishStateAsync(state, BaseTopic);
                _logger.LogInfo($"[{Name}] State changed to: {state}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{Name}] Failed to send state '{state}': {ex.Message}");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}