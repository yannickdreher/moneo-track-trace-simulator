using App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace App.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        private async void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.SaveAsync();
        }
    }
}