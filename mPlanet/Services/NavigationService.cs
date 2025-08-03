using mPlanet.Services.Interfaces;
using mPlanet.Views.Pages;
using mPlanet.ViewModels;
using mPlanet.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace mPlanet.Services
{
    public class NavigationService : INavigationService
    {
        private Frame _frame;
        private string _currentPageName;
        
        // Cache for reusing pages
        private readonly Dictionary<string, Page> _pageCache = new Dictionary<string, Page>();

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
                Page page = GetOrCreatePage(pageName);
                _currentPageName = pageName;

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
                else
                {
                    // Fallback to URI navigation for unknown pages
                    var uri = new Uri($"Views/Pages/{pageName}.xaml", UriKind.Relative);
                    _frame.Navigate(uri);
                    OnNavigationChanged($"Navigated to: {pageName}");
                }
            }
            catch (Exception ex)
            {
                OnNavigationChanged($"Navigation error: {ex.Message}");
            }
        }

        private Page GetOrCreatePage(string pageName)
        {
            // Check if page already exists in cache
            if (_pageCache.ContainsKey(pageName))
            {
                return _pageCache[pageName];
            }

            // Create new page and add to cache
            Page page = null;
            
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

                case "StockTakePage":
                    var stockTakePage = new StockTakePage();
                    var stockTakePageViewModel = new StockTakePageViewModel(this);
                    stockTakePage.SetViewModel(stockTakePageViewModel);
                    page = stockTakePage;
                    break;

                case "HelpPage":
                    // Add when you create this page
                    break;

                default:
                    // For unknown pages, return null and let the caller handle it
                    // or you could throw an exception for unsupported pages
                    return null;
            }

            if (page != null)
            {
                _pageCache[pageName] = page;
            }

            return page;
        }

        public void NavigateTo(Type pageType)
        {
            if (_frame == null) return;

            try
            {
                // For type-based navigation, we'll still create new instances
                // as we don't have a string key to cache by
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

        public void ClearPageCache()
        {
            // Call OnNavigatedFrom for all cached pages before clearing
            foreach (var cachedPage in _pageCache.Values)
            {
                if (cachedPage.DataContext is PageViewModelBase viewModel)
                {
                    viewModel.OnNavigatedFrom();
                }
            }
            
            _pageCache.Clear();
        }

        public void RemovePageFromCache(string pageName)
        {
            if (_pageCache.ContainsKey(pageName))
            {
                var page = _pageCache[pageName];
                if (page.DataContext is PageViewModelBase viewModel)
                {
                    viewModel.OnNavigatedFrom();
                }
                _pageCache.Remove(pageName);
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