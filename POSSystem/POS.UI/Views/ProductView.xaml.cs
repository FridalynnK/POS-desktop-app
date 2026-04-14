using Microsoft.Extensions.DependencyInjection;
using POS.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;  // ← ADD THIS
namespace POS.UI.Views
{
    public partial class ProductView : UserControl
    {
        private readonly ProductViewModel _viewModel;

        public ProductView()
        {
            InitializeComponent();
            _viewModel = App.ServiceProvider!.GetRequiredService<ProductViewModel>();
            DataContext = _viewModel;

            Loaded += async (s, e) => await _viewModel.LoadProductsAsync();
        }
    }
}
