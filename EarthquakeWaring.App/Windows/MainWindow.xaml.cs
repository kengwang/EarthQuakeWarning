using System;
using System.ComponentModel;
using System.Windows;
using EarthquakeWaring.App.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EarthquakeWaring.App.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _services;
        private readonly IHostApplicationLifetime _lifetime;

        public MainWindow(IServiceProvider services, IHostApplicationLifetime lifetime)
        {
            _services = services;
            _lifetime = lifetime;
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

        private void OnNavigateExamples(object sender, RoutedEventArgs e)
        {
            RootFrame.Navigate(_services.GetService<EarthQuakeExamplesPage>());
        }
    }
}