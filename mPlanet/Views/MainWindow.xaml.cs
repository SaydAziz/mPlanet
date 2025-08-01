using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Net;
using MegawareDLL;
using Newtonsoft.Json;
using System.Text;
using mPlanet.Models;
using mPlanet.ViewModels;

namespace mPlanet.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double minEpcColumnWidth = 200;
            double minRssiColumnWidth = 120;
            double remainingWidth = scannedTagsListView.ActualWidth - SystemParameters.VerticalScrollBarWidth;

            if (remainingWidth > (minEpcColumnWidth + minRssiColumnWidth))
            {
                double newWidth = remainingWidth / 2;
                epcColumn.Width = newWidth > minEpcColumnWidth ? newWidth : minEpcColumnWidth;
                rssiColumn.Width = newWidth > minRssiColumnWidth ? newWidth : minRssiColumnWidth;
            }
            else
            {
                epcColumn.Width = minEpcColumnWidth;
                rssiColumn.Width = minRssiColumnWidth;
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_viewModel != null)
            {
                await _viewModel.CleanupAsync();
            }
        }
    }
}
