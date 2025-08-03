using System.Windows.Input;
using mPlanet.Infrastructure;
using mPlanet.Services.Interfaces;

namespace mPlanet.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private string _statusMessage = "Готов";
        private string _connectionStatus = "Не подключен";
        private bool _isConnected = false;

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set 
            { 
                if (SetProperty(ref _isConnected, value))
                {
                    UpdateConnectionStatusDisplay();
                }
            }
        }

        // Navigation Commands
        public ICommand NavigateToMainCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand NavigateToStockTakeCommand { get; }
        public ICommand NavigateToHelpCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand GoForwardCommand { get; }

        public MainWindowViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            // Subscribe to navigation events
            _navigationService.NavigationChanged += OnNavigationChanged;
            // Subscribe to status message updates from pages
            _navigationService.StatusMessageChanged += OnStatusMessageChanged;
            // Subscribe to connection status updates
            _navigationService.ConnectionStatusChanged += OnConnectionStatusChanged;

            // Initialize commands
            NavigateToMainCommand = new RelayCommand(_ => NavigateToPage("MainPage", "Главная"));
            NavigateToSettingsCommand = new RelayCommand(_ => NavigateToPage("SettingsPage", "Настройки"));
            NavigateToStockTakeCommand = new RelayCommand(_ => NavigateToPage("StockTakePage", "Инвентаризация"));
            NavigateToHelpCommand = new RelayCommand(_ => NavigateToPage("HelpPage", "Справка"));
            
            GoBackCommand = new RelayCommand(
                execute: _ => _navigationService.GoBack(),
                canExecute: _ => _navigationService.CanGoBack);
            
            GoForwardCommand = new RelayCommand(
                execute: _ => _navigationService.GoForward(),
                canExecute: _ => _navigationService.CanGoForward);
        }

        private void NavigateToPage(string pageName, string displayName)
        {
            _navigationService.NavigateTo(pageName);
            StatusMessage = $"Перешли к: {displayName}";
        }

        private void OnNavigationChanged(object sender, string message)
        {
            // Only update status message for navigation, not connection status
            StatusMessage = message;
        }

        private void OnStatusMessageChanged(object sender, string message)
        {
            StatusMessage = message;
        }

        private void OnConnectionStatusChanged(object sender, (bool isConnected, string comPort) status)
        {
            IsConnected = status.isConnected;
            
            if (status.isConnected)
            {
                ConnectionStatus = $"Подключен: {status.comPort}";
            }
            else
            {
                ConnectionStatus = "Не подключен";
            }
        }

        private void UpdateConnectionStatusDisplay()
        {
            if (IsConnected)
            {
                // Connection status will be updated via the event with COM port info
            }
            else
            {
                ConnectionStatus = "Не подключен";
            }
        }
    }
}