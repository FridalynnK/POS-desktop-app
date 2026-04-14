using POS.UI.ViewModels;
using System.Windows.Controls;

namespace POS.UI.Views
{
    public partial class UserManagementView : UserControl
    {
        private readonly UserManagementViewModel _viewModel;

        public UserManagementView(UserManagementViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            Loaded += async (_, _) => await _viewModel.LoadAsync();
        }
    }
}
