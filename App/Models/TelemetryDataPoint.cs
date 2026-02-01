using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace App.Models
{
    public partial class TelemetryDataPoint : INotifyPropertyChanged
    {
        private string _identifier = string.Empty;
        private string _unit = string.Empty;
        private double _value;
        private bool _useRandom;
        private double _minValue;
        private double _maxValue;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Identifier
        {
            get => _identifier;
            set
            {
                if (_identifier != value)
                {
                    _identifier = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Unit
        {
            get => _unit;
            set
            {
                if (_unit != value)
                {
                    _unit = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UseRandom
        {
            get => _useRandom;
            set
            {
                if (_useRandom != value)
                {
                    _useRandom = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanEditValue));
                }
            }
        }

        public double MinValue
        {
            get => _minValue;
            set
            {
                if (_minValue != value)
                {
                    _minValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public double MaxValue
        {
            get => _maxValue;
            set
            {
                if (_maxValue != value)
                {
                    _maxValue = value;
                    OnPropertyChanged();
                }
            }
        }

        // Computed property for UI binding
        public bool CanEditValue => !UseRandom;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}