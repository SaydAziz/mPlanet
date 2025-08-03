using System.Windows.Controls;
using mPlanet.ViewModels.Pages;

namespace mPlanet.Views.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public void SetViewModel(MainPageViewModel viewModel)
        {
            DataContext = viewModel;
        }
    }
}
