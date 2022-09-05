using System;
using System.ComponentModel;
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

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            App.RootFrame = RootFrame;
            App.MainWindowOpened = true;
            RootFrame.Navigate(new WelcomePage());
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

        private void MainWindow_OnClosed(object? sender, EventArgs e)
        {
            App.MainWindowOpened = false;
            App.RootFrame = null;
        }
    }
}