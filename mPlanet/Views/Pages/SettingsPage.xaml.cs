using System.Windows.Controls;
using mPlanet.ViewModels;

namespace mPlanet.Views.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        public void SetViewModel(SettingsPageViewModel viewModel)
        {
            DataContext = viewModel;
        }
    }
}