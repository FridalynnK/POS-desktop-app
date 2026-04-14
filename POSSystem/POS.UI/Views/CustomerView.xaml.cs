using POS.UI.ViewModels;
using System.Windows.Controls;

namespace POS.UI.Views
{
    public partial class CustomerView : UserControl
    {
        private readonly CustomerViewModel _vm;

        public CustomerView(CustomerViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
            Loaded += async (_, _) => await _vm.LoadAsync();
        }


    }
}
