using App.Models;
using App.Settings;
using App.ViewModels;

namespace App.Services
{
    public class StationViewModelFactory(
        StationService stationService,
        LogService logService,
        SettingsService<MqttSettings> settingsService)
    {
        private readonly StationService _stationService = stationService;
        private readonly LogService _logger = logService;
        private readonly SettingsService<MqttSettings> _settingsService = settingsService;

        public StationViewModel CreateStation(string name, string topicSuffix)
        {
            var model = new StationModel
            {
                Name = name,
                BaseTopic = $"{_settingsService.Value.Topic}/{topicSuffix}",
                TelemetryIntervalSeconds = 1
            };

            var viewModel = new StationViewModel(model, _stationService, _logger);
            viewModel.InitializeDefaultCarriers();

            return viewModel;
        }
    }
}