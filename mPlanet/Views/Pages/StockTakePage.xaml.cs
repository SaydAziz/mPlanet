using System.Windows.Controls;
using mPlanet.ViewModels.Pages;

namespace mPlanet.Views.Pages
{
    /// <summary>
    /// Interaction logic for StockTakePage.xaml
    /// </summary>
    public partial class StockTakePage : Page
    {
        public StockTakePage()
        {
            InitializeComponent();
        }

        public void SetViewModel(StockTakePageViewModel viewModel)
        {
            DataContext = viewModel;
        }
    }
}
