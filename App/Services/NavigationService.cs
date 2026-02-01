using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;

namespace App.Services
{
    public class NavigationService(IServiceProvider serviceProvider)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private Frame? _frame;

        public void SetFrame(Frame frame)
        {
            _frame = frame;
        }

        public void NavigateTo<T>() where T : Page
        {
            if (_frame == null)
                throw new InvalidOperationException("Frame is not set. Call SetFrame first.");

            var page = _serviceProvider.GetRequiredService<T>();
            _frame.Content = page;
        }

        public void NavigateTo(Type pageType)
        {
            if (_frame == null)
                throw new InvalidOperationException("Frame is not set. Call SetFrame first.");

            var page = _serviceProvider.GetRequiredService(pageType) as Page;
            _frame.Content = page;
        }
    }
}