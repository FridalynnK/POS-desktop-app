using POS.Core.Entities;
using POS.Core.Interfaces;
using POS.Services.Auth;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using POS.UI.Commands;  // instead of POS.UI.ViewModels

namespace POS.UI.ViewModels
{
    public class DebtViewModel : INotifyPropertyChanged
    {
        private readonly IDebtService    _debtService;
        private readonly IReceiptService _receiptService;
        private readonly SessionContext  _session;

        // ── Customer list ─────────────────────────────────────────────
        public ObservableCollection<Customer>        Customers     { get; } = new();
        public ObservableCollection<CustomerBalance> Debts         { get; } = new();
        public ObservableCollection<Payment>         DebtPayments  { get; } = new();

        // ── Selected state ────────────────────────────────────────────
        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged();
                SelectedDebt = null;
                _ = LoadDebtsAsync();
            }
        }

        private CustomerBalance? _selectedDebt;
        public CustomerBalance? SelectedDebt
        {
            get => _selectedDebt;
            set
            {
                _selectedDebt = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedDebt));
                OnPropertyChanged(nameof(SelectedDebtSummary));
                LoadDebtPayments();
            }
        }

        public bool HasSelectedDebt => _selectedDebt != null;

        public string SelectedDebtSummary => _selectedDebt == null
            ? string.Empty
            : $"Invoice {_selectedDebt.Sale?.Reference ?? "—"}  |  " +
              $"Total: XAF {_selectedDebt.TotalAmount:N0}  |  " +
              $"Outstanding: XAF {_selectedDebt.Outstanding:N0}";

        // ── Repayment form ────────────────────────────────────────────
        private string _repaymentAmount = string.Empty;
        public string RepaymentAmount
        {
            get => _repaymentAmount;
            set { _repaymentAmount = value; OnPropertyChanged(); }
        }

        private string _repaymentMethod = "Cash";
        public string RepaymentMethod
        {
            get => _repaymentMethod;
            set { _repaymentMethod = value; OnPropertyChanged(); }
        }

        private string _repaymentNotes = string.Empty;
        public string RepaymentNotes
        {
            get => _repaymentNotes;
            set { _repaymentNotes = value; OnPropertyChanged(); }
        }

        // ── Summary totals ────────────────────────────────────────────
        private decimal _totalOutstanding;
        public decimal TotalOutstanding
        {
            get => _totalOutstanding;
            set { _totalOutstanding = value; OnPropertyChanged(); }
        }

        private int _totalDebtors;
        public int TotalDebtors
        {
            get => _totalDebtors;
            set { _totalDebtors = value; OnPropertyChanged(); }
        }

        // ── Status ────────────────────────────────────────────────────
        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private bool _isError;
        public bool IsError
        {
            get => _isError;
            set { _isError = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        // ── Commands ──────────────────────────────────────────────────
        public ICommand RecordPaymentCommand    { get; }
        public ICommand PrintDebtReceiptCommand { get; }
        public ICommand RefreshCommand          { get; }
        public ICommand SelectCustomerCommand   { get; }
        public ICommand SelectDebtCommand       { get; }

        public DebtViewModel(IDebtService debtService, IReceiptService receiptService, SessionContext session)
        {
            _debtService    = debtService;
            _receiptService = receiptService;
            _session        = session;

            RecordPaymentCommand    = new AsyncRelayCommand<object>(_ => ExecuteRecordPaymentAsync());
            PrintDebtReceiptCommand = new AsyncRelayCommand<object>(_ => ExecutePrintDebtReceiptAsync());
            RefreshCommand          = new AsyncRelayCommand<object>(_ => LoadAsync());
            SelectCustomerCommand   = new RelayCommand<Customer>(c => SelectedCustomer = c);
            SelectDebtCommand       = new RelayCommand<CustomerBalance>(d => SelectedDebt = d);
        }

        public async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                // Load registered customers
                var customers = await _debtService.GetRegisteredCustomersAsync();
                Customers.Clear();
                foreach (var c in customers)
                    Customers.Add(c);

                // Load all debts for summary totals
                var allDebts = await _debtService.GetOutstandingDebtsAsync();
                TotalOutstanding = allDebts.Sum(d => d.Outstanding);
                TotalDebtors     = allDebts.Select(d => d.CustomerId).Distinct().Count();

                // Reload debts for selected customer if any
                await LoadDebtsAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadDebtsAsync()
        {
            var debts = await _debtService.GetOutstandingDebtsAsync(
                _selectedCustomer?.Id);

            Debts.Clear();
            foreach (var d in debts)
                Debts.Add(d);
        }

        private void LoadDebtPayments()
        {
            DebtPayments.Clear();
            if (_selectedDebt?.Payments == null) return;
            foreach (var p in _selectedDebt.Payments.OrderByDescending(p => p.PaymentDate))
                DebtPayments.Add(p);
        }

        private async Task ExecuteRecordPaymentAsync()
        {
            if (_selectedDebt == null)
            {
                ShowStatus("Please select a debt first.", error: true);
                return;
            }

            if (!decimal.TryParse(RepaymentAmount, out decimal amount) || amount <= 0)
            {
                ShowStatus("Enter a valid payment amount.", error: true);
                return;
            }

            if (amount > _selectedDebt.Outstanding)
            {
                ShowStatus($"Amount exceeds outstanding balance (XAF {_selectedDebt.Outstanding:N0}).", error: true);
                return;
            }

            IsBusy = true;
            try
            {
                await _debtService.RecordPaymentAsync(
                    customerBalanceId: _selectedDebt.Id,
                    amount:            amount,
                    paymentMethod:     RepaymentMethod,
                    cashierId:         _session.CurrentUser!.Id,
                    notes:             RepaymentNotes);

                RepaymentAmount = string.Empty;
                RepaymentNotes  = string.Empty;

                ShowStatus($"Payment of XAF {amount:N0} recorded successfully.", error: false);

                await LoadAsync();
            }
            catch (Exception ex)
            {
                ShowStatus(ex.Message, error: true);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecutePrintDebtReceiptAsync()
        {
            if (_selectedDebt == null)
            {
                ShowStatus("Please select a debt to reprint.", error: true);
                return;
            }

            IsBusy = true;
            try
            {
                decimal amountPaid = _selectedDebt.TotalAmount - _selectedDebt.Outstanding;
                await _receiptService.PrintDebtReceiptAsync(
                    _selectedDebt.SaleId,
                    amountPaid,
                    _selectedDebt.DueDate);

                ShowStatus("Debt receipt sent to printer.", error: false);
            }
            catch (Exception ex)
            {
                ShowStatus(ex.Message, error: true);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ShowStatus(string message, bool error)
        {
            StatusMessage = message;
            IsError       = error;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
