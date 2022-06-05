using System;
using System.Windows;
using EarthquakeWaring.App.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace EarthquakeWaring.App.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _services;

        public MainWindow(IServiceProvider services)
        {
            _services = services;
            InitializeComponent();
        }

        private void OnNavigatingSettings(object sender, RoutedEventArgs e)
        {
            RootFrame.Navigate(_services.GetService<SettingsPage>());
        }

        private void OnNavigatingEarthQuakesList(object sender, RoutedEventArgs e)
        {
            RootFrame.Navigate(_services.GetService<EarthQuakesListPage>());
        }
    }
}