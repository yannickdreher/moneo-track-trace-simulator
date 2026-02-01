using Microsoft.UI.Xaml.Controls;
using App.Services;
using App.Models;
using App.ViewModels;

namespace App.Pages
{
    public sealed partial class SimulationPage : Page
    {
        private readonly StationViewModelFactory _stationFactory;

        private StationViewModel? _initializationStation;
        private StationViewModel? _mixerStation;
        private StationViewModel? _warehouseStation;

        public SimulationPage(StationViewModelFactory stationFactory)
        {
            InitializeComponent();

            _stationFactory = stationFactory;

            InitializeStations();
        }

        private void InitializeStations()
        {
            // Initialize Initialization Station
            _initializationStation = _stationFactory.CreateStation(
                "Initialization",
                "station/initialization");

            _initializationStation.TelemetryDataPoints.Add(new TelemetryDataPoint
            {
                Identifier = "Power",
                Unit = "Wh",
                Value = 500,
                UseRandom = true,
                MinValue = 300,
                MaxValue = 600
            });
            _initializationStation.Attributes.Add(new AttributeDataPoint
            {
                Identifier = "OrderNumber",
                Value = string.Empty
            });

            InitializationStation.ViewModel = _initializationStation;

            // Initialize Mixer Station
            _mixerStation = _stationFactory.CreateStation(
                "Mixer",
                "station/mixer");

            _mixerStation.TelemetryIntervalSeconds = 1;
            _mixerStation.TelemetryDataPoints.Add(new TelemetryDataPoint
            {
                Identifier = "AgitatorSpeed",
                Unit = "rpm",
                Value = 1500,
                UseRandom = true,
                MinValue = 1000,
                MaxValue = 2000
            });
            _mixerStation.Attributes.Add(new AttributeDataPoint
            {
                Identifier = "AgitatorId",
                Value = string.Empty
            });

            MixerStation.ViewModel = _mixerStation;

            // Initialize Warehouse Station
            _warehouseStation = _stationFactory.CreateStation(
                "Warehouse",
                "station/warehouse");

            _warehouseStation.TelemetryDataPoints.Add(new TelemetryDataPoint
            {
                Identifier = "Temperature",
                Unit = "°C",
                Value = 25.0,
                UseRandom = true,
                MinValue = 20,
                MaxValue = 30
            });

            WarehouseStation.ViewModel = _warehouseStation;
        }
    }
}