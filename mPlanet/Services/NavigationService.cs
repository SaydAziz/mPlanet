using mPlanet.Services.Interfaces;
using mPlanet.Views.Pages;
using mPlanet.ViewModels;
using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace mPlanet.Services
{
    public class NavigationService : INavigationService
    {
        private Frame _frame;
        private string _currentPageName;

        public bool CanGoBack => _frame?.CanGoBack ?? false;
        public bool CanGoForward => _frame?.CanGoForward ?? false;

        public event EventHandler<string> NavigationChanged;
        public event EventHandler<string> StatusMessageChanged;
        public event EventHandler<(bool isConnected, string comPort)> ConnectionStatusChanged;

        public void Initialize(Frame frame)
        {
            _frame = frame;
            if (_frame != null)
            {
                _frame.Navigated += OnNavigated;
            }
        }

        public void UpdateStatusMessage(string message)
        {
            StatusMessageChanged?.Invoke(this, message);
        }

        public void UpdateConnectionStatus(bool isConnected, string comPort = "")
        {
            ConnectionStatusChanged?.Invoke(this, (isConnected, comPort));
        }

        public void NavigateTo(string pageName)
        {
            if (_frame == null) return;

            try
            {
                Page page = null;
                _currentPageName = pageName;

                // Create page instances with their ViewModels
                switch (pageName)
                {
                    case "MainPage":
                        var mainPage = new MainPage();
                        var mainPageViewModel = new MainPageViewModel(this);
                        mainPage.SetViewModel(mainPageViewModel);
                        page = mainPage;
                        break;

                    case "SettingsPage":
                        var settingsPage = new SettingsPage();
                        var settingsPageViewModel = new SettingsPageViewModel(this);
                        settingsPage.SetViewModel(settingsPageViewModel);
                        page = settingsPage;
                        break;

                    default:
                        // Fallback to URI navigation for unknown pages
                        var uri = new Uri($"Views/Pages/{pageName}.xaml", UriKind.Relative);
                        _frame.Navigate(uri);
                        OnNavigationChanged($"Navigated to: {pageName}");
                        return;
                }

                if (page != null)
                {
                    _frame.Navigate(page);
                    
                    // Call lifecycle methods if the page has a ViewModel
                    if (page.DataContext is PageViewModelBase viewModel)
                    {
                        viewModel.OnNavigatedTo();
                    }
                    
                    OnNavigationChanged($"Successfully navigated to: {pageName}");
                }
            }
            catch (Exception ex)
            {
                OnNavigationChanged($"Navigation error: {ex.Message}");
            }
        }

        public void NavigateTo(Type pageType)
        {
            if (_frame == null) return;

            try
            {
                var page = Activator.CreateInstance(pageType);
                _frame.Navigate(page);
                OnNavigationChanged($"Navigated to: {pageType.Name}");
            }
            catch (Exception ex)
            {
                OnNavigationChanged($"Navigation error: {ex.Message}");
            }
        }

        public void GoBack()
        {
            if (CanGoBack)
            {
                _frame.GoBack();
                OnNavigationChanged("Navigated back");
            }
        }

        public void GoForward()
        {
            if (CanGoForward)
            {
                _frame.GoForward();
                OnNavigationChanged("Navigated forward");
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            // Handle navigation lifecycle for ViewModels
            if (e.Content is Page page && page.DataContext is PageViewModelBase viewModel)
            {
                viewModel.OnNavigatedTo();
            }
        }

        private void OnNavigationChanged(string message)
        {
            NavigationChanged?.Invoke(this, message);
        }
    }
}