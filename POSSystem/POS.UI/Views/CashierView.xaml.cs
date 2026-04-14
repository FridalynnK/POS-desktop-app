using Microsoft.Extensions.DependencyInjection;
using POS.UI.ViewModels;
using System.Windows.Controls;

namespace POS.UI.Views
{
    public partial class CashierView : UserControl
    {
        private readonly CashierViewModel _viewModel;

        public CashierView()
        {
            InitializeComponent();
            _viewModel = App.ServiceProvider!.GetRequiredService<CashierViewModel>();
            DataContext = _viewModel;

            Loaded += async (s, e) => await _viewModel.LoadProductsAsync();
        }
    }
}
