using POS.Core.Entities;
using POS.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;  // ← ADD THIS
using System.Windows.Input;

namespace POS.UI.Views
{
    public partial class DebtManagementView : UserControl
    {
        private readonly DebtViewModel _viewModel;

        public DebtManagementView(DebtViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            Loaded += async (_, _) => await _viewModel.LoadAsync(); // ← REPLACES OnContentRendered
        }
        // DELETE the OnContentRendered override entirely
    }
}