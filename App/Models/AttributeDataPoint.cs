using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace App.Models
{
    public partial class AttributeDataPoint : INotifyPropertyChanged
    {
        private string _identifier = string.Empty;
        private string _value = string.Empty;

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

        public string Value
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

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}