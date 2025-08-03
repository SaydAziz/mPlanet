using System.Windows;
using mPlanet.Services;
using mPlanet.ViewModels;

namespace mPlanet.Views
{
    public partial class MainWindow : Window
    {
        private NavigationService _navigationService;
        private MainWindowViewModel _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            InitializeNavigation();
        }

        private void InitializeNavigation()
        {
            // Initialize navigation service
            _navigationService = new NavigationService();
            _navigationService.Initialize(MainFrame);

            // Create main window ViewModel
            _mainViewModel = new MainWindowViewModel(_navigationService);
            DataContext = _mainViewModel;

            // Navigate to initial page
            _navigationService.NavigateTo("MainPage");
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Cleanup when closing
        }
    }
}