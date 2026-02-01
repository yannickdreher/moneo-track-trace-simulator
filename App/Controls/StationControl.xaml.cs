using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using App.ViewModels;

namespace App.Controls
{
    public sealed partial class StationControl : UserControl
    {
        // Dependency Properties
        public static readonly DependencyProperty StationTitleProperty =
            DependencyProperty.Register(nameof(StationTitle), typeof(string), typeof(StationControl), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(nameof(Image), typeof(string), typeof(StationControl), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register(nameof(BorderColor), typeof(Brush), typeof(StationControl), new PropertyMetadata(null));

        public static readonly DependencyProperty HasAttributesProperty =
            DependencyProperty.Register(nameof(HasAttributes), typeof(bool), typeof(StationControl), new PropertyMetadata(false));

        public static readonly DependencyProperty TelemetrySectionProperty =
            DependencyProperty.Register(nameof(TelemetrySection), typeof(object), typeof(StationControl), new PropertyMetadata(null));

        public static readonly DependencyProperty AttributesSectionProperty =
            DependencyProperty.Register(nameof(AttributesSection), typeof(object), typeof(StationControl), new PropertyMetadata(null));

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(StationViewModel),
                typeof(StationControl),
                new PropertyMetadata(null));

        // Properties
        public string StationTitle
        {
            get => (string)GetValue(StationTitleProperty);
            set => SetValue(StationTitleProperty, value);
        }

        public string Image
        {
            get => (string)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        public Brush BorderColor
        {
            get => (Brush)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        public bool HasAttributes
        {
            get => (bool)GetValue(HasAttributesProperty);
            set => SetValue(HasAttributesProperty, value);
        }

        public object TelemetrySection
        {
            get => GetValue(TelemetrySectionProperty);
            set => SetValue(TelemetrySectionProperty, value);
        }

        public object AttributesSection
        {
            get => GetValue(AttributesSectionProperty);
            set => SetValue(AttributesSectionProperty, value);
        }

        public StationViewModel ViewModel
        {
            get => (StationViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public StationControl()
        {
            InitializeComponent();
        }

        private void AddCarrier_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(CarrierTextBox.Text))
            {
                ViewModel?.AddCarrier(CarrierTextBox.Text);
                CarrierTextBox.Text = string.Empty;
            }
        }

        private void RemoveCarrier_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: string carrierName })
            {
                ViewModel?.RemoveCarrier(carrierName);
            }
        }

        private async void Entry_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.SendEntryAsync();
            }
        }

        private async void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.SendExitAsync();
            }
        }

        private void StartTelemetry_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.StartTelemetry();
        }

        private void StopTelemetry_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.StopTelemetry();
        }

        private async void SendAttributes_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.SendAttributesAsync();
            }
        }

        private async void SendMachineState_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null && sender is Button { Tag: string state })
            {
                await ViewModel.SendStateAsync(state);
            }
        }
    }
}