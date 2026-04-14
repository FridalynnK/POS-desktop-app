using POS.Core.Entities;
using POS.Core.Interfaces;
using POS.UI.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace POS.UI.ViewModels
{
    public class CustomerViewModel : INotifyPropertyChanged
    {
        private readonly ICustomerService _customerService;

        public ObservableCollection<Customer> Customers { get; set; } = new();

        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged();
                if (value != null) LoadFormFrom(value);
            }
        }

        private string _name = "", _phone = "", _address = "", _notes = "", _status = "";
        private bool _isEditing;

        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        public string Phone { get => _phone; set { _phone = value; OnPropertyChanged(); } }
        public string Address { get => _address; set { _address = value; OnPropertyChanged(); } }
        public string Notes { get => _notes; set { _notes = value; OnPropertyChanged(); } }
        public string StatusMessage { get => _status; set { _status = value; OnPropertyChanged(); } }
        public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand CancelCommand { get; }

        public CustomerViewModel(ICustomerService customerService)
        {
            _customerService = customerService;

            SaveCommand = new RelayCommand(async () => await SaveAsync());
            DeleteCommand = new RelayCommand(
                                async () => await DeleteAsync(),
                                () => SelectedCustomer != null);
            NewCommand = new RelayCommand(ClearForm);
            CancelCommand = new RelayCommand(ClearForm);
        }

        public async Task LoadAsync()
        {
            var list = await _customerService.GetAllCustomersAsync();

            // Must run on UI thread when updating ObservableCollection
            App.Current.Dispatcher.Invoke(() =>
            {
                Customers.Clear();
                foreach (var c in list) Customers.Add(c);
            });
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            { StatusMessage = "Name is required."; return; }

            if (IsEditing && SelectedCustomer != null)
            {
                SelectedCustomer.Name = Name;
                SelectedCustomer.Phone = Phone;
                SelectedCustomer.Address = Address;
                SelectedCustomer.Notes = Notes;
                await _customerService.UpdateCustomerAsync(SelectedCustomer);
                StatusMessage = "Customer updated.";
            }
            else
            {
                var c = new Customer
                {
                    Name = Name,
                    Phone = Phone,
                    Address = Address,
                    Notes = Notes
                };
                await _customerService.AddCustomerAsync(c);
                StatusMessage = "Customer added.";
            }

            ClearForm();
            await LoadAsync();  // reload after clear
        }

        private async Task DeleteAsync()
        {
            if (SelectedCustomer == null) return;
            var name = SelectedCustomer.Name;
            await _customerService.DeleteCustomerAsync(SelectedCustomer.Id);
            StatusMessage = $"Deleted {name}.";
            ClearForm();
            await LoadAsync();
        }

        private void LoadFormFrom(Customer c)
        {
            Name = c.Name ?? "";
            Phone = c.Phone ?? "";
            Address = c.Address ?? "";
            Notes = c.Notes ?? "";
            IsEditing = true;
        }

        private void ClearForm()
        {
            _selectedCustomer = null;   // bypass setter to avoid re-triggering LoadFormFrom
            OnPropertyChanged(nameof(SelectedCustomer));
            Name = Phone = Address = Notes = string.Empty;
            IsEditing = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}