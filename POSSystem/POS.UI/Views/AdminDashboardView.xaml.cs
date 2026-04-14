using Microsoft.Extensions.DependencyInjection;
using POS.UI.ViewModels;
using System.Windows.Controls;

namespace POS.UI.Views
{
    public partial class AdminDashboardView : UserControl

    {
        private readonly DashboardViewModel _viewModel;

        public AdminDashboardView(DashboardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            Loaded += async (_, _) => await _viewModel.LoadAsync();
        }
    }
}
