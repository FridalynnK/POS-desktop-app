using POS.Core.DTOs;
using POS.Core.Entities;
using POS.Core.Interfaces;
using POS.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace POS.UI.ViewModels
{
    public class CashierViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly ISaleService _saleService;
        private readonly IReceiptService _receiptService;
        private readonly IProductService _productService;

        // ── Products (left panel) ────────────────────────────────────────────
        private ObservableCollection<Product> _allProducts = new();
        public ObservableCollection<Product> Products { get; set; } = new();

        private Product? _selectedProduct;
        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set { _selectedProduct = value; OnPropertyChanged(); }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterProducts();
            }
        }

        // ── Cart (right panel) ───────────────────────────────────────────────
        public ObservableCollection<SaleItemDto> CartItems { get; set; } = new();

        private SaleItemDto? _selectedCartItem;
        public SaleItemDto? SelectedCartItem
        {
            get => _selectedCartItem;
            set { _selectedCartItem = value; OnPropertyChanged(); }
        }

        // ── Totals ───────────────────────────────────────────────────────────
        public decimal GrandTotal => CartItems.Sum(x => x.LineTotal);

        private decimal _cashAmountEntered;
        public decimal CashAmountEntered
        {
            get => _cashAmountEntered;
            set
            {
                _cashAmountEntered = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ChangeDue));
                OnPropertyChanged(nameof(ShowChangeDue));
            }
        }

        public decimal ChangeDue => CashAmountEntered > GrandTotal
                                        ? CashAmountEntered - GrandTotal : 0;
        public bool ShowChangeDue => SelectedPaymentMethod == "Cash" && ChangeDue > 0;

        // ── Payment ──────────────────────────────────────────────────────────
        public List<string> PaymentMethods { get; } =
            new List<string> { "Cash", "MobileMoney", "Debt" };

        private string _selectedPaymentMethod = "Cash";
        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                _selectedPaymentMethod = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCashPayment));
                OnPropertyChanged(nameof(ShowChangeDue));
            }
        }

        public bool IsCashPayment => SelectedPaymentMethod == "Cash";

        // ── Commands ─────────────────────────────────────────────────────────
        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand CompleteSaleCommand { get; }
        public ICommand ClearCartCommand { get; }

        // ── Constructor ──────────────────────────────────────────────────────
        public CashierViewModel(ISaleService saleService,
                                IReceiptService receiptService,
                                IProductService productService)
        {
            _saleService = saleService;
            _receiptService = receiptService;
            _productService = productService;

            AddToCartCommand = new RelayCommand(OnAddToCart,
                                        () => SelectedProduct != null);
            RemoveFromCartCommand = new RelayCommand(OnRemoveFromCart,
                                        () => SelectedCartItem != null);
            CompleteSaleCommand = new RelayCommand(async () => await OnCompleteSaleAsync(),
                                        () => CartItems.Any());
            ClearCartCommand = new RelayCommand(OnClearCart,
                                        () => CartItems.Any());
        }

        // ── Load products ────────────────────────────────────────────────────
        public async Task LoadProductsAsync()
        {
            var products = await _productService.GetAllProductsAsync();
            _allProducts = new ObservableCollection<Product>(products);
            FilterProducts();
        }

        private void FilterProducts()
        {
            Products.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _allProducts
                : new ObservableCollection<Product>(
                    _allProducts.Where(p =>
                        (p.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (p.SKU?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)));

            foreach (var p in filtered)
                Products.Add(p);
        }

        // ── Add to cart ──────────────────────────────────────────────────────
        private void OnAddToCart()
        {
            if (SelectedProduct == null) return;

            var input = Microsoft.VisualBasic.Interaction.InputBox(
                $"Enter quantity for {SelectedProduct.Name}:", "Quantity", "1");

            if (!int.TryParse(input, out int quantity) || quantity <= 0) return;

            if (quantity > SelectedProduct.Quantity)
            {
                MessageBox.Show($"Only {SelectedProduct.Quantity} units in stock.",
                    "Insufficient Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existing = CartItems.FirstOrDefault(x => x.ProductId == SelectedProduct.Id);
            if (existing != null)
            {
                var newQty = existing.Quantity + quantity;
                if (newQty > SelectedProduct.Quantity)
                {
                    MessageBox.Show(
                        $"Total quantity would exceed available stock ({SelectedProduct.Quantity}).",
                        "Insufficient Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                existing.Quantity = newQty;
                var idx = CartItems.IndexOf(existing);
                CartItems[idx] = existing;
            }
            else
            {
                CartItems.Add(new SaleItemDto
                {
                    ProductId = SelectedProduct.Id,
                    Name = SelectedProduct.Name,
                    Quantity = quantity,
                    UnitPrice = SelectedProduct.SellingPrice
                });
            }

            OnPropertyChanged(nameof(GrandTotal));
            OnPropertyChanged(nameof(ChangeDue));
            OnPropertyChanged(nameof(ShowChangeDue));
        }

        // ── Remove from cart ─────────────────────────────────────────────────
        private void OnRemoveFromCart()
        {
            if (SelectedCartItem == null) return;
            CartItems.Remove(SelectedCartItem);
            SelectedCartItem = null;
            OnPropertyChanged(nameof(GrandTotal));
            OnPropertyChanged(nameof(ChangeDue));
            OnPropertyChanged(nameof(ShowChangeDue));
        }

        // ── Clear cart ───────────────────────────────────────────────────────
        private void OnClearCart()
        {
            var result = MessageBox.Show("Clear all items from the cart?", "Clear Cart",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            CartItems.Clear();
            CashAmountEntered = 0;
            OnPropertyChanged(nameof(GrandTotal));
        }

        // ── Complete sale ────────────────────────────────────────────────────
        private async Task OnCompleteSaleAsync()
        {
            if (!CartItems.Any())
            {
                MessageBox.Show("Cart is empty.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedPaymentMethod))
            {
                MessageBox.Show("Please select a payment method.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedPaymentMethod == "Cash" && CashAmountEntered < GrandTotal)
            {
                MessageBox.Show(
                    $"Cash entered ({CashAmountEntered:C}) is less than total ({GrandTotal:C}).",
                    "Insufficient Cash", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedPaymentMethod == "Debt")
            {
                var confirm = MessageBox.Show(
                    $"Record this sale of {GrandTotal:C} as a debt?",
                    "Confirm Debt Sale", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm != MessageBoxResult.Yes) return;
            }

            try
            {
                var request = new SaleRequestDto
                {
                    CashierId = 1,
                    CustomerId = null,
                    PaymentMethod = SelectedPaymentMethod,
                    Items = CartItems.ToList()
                };

                var saleId = await _saleService.CreateSaleAsync(request);
                await _receiptService.PrintReceiptAsync(saleId);

                if (SelectedPaymentMethod == "Cash" && ChangeDue > 0)
                    MessageBox.Show($"Sale complete!\nChange due: {ChangeDue:C}",
                        "Sale Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Sale completed successfully!",
                        "Sale Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                CartItems.Clear();
                CashAmountEntered = 0;
                SelectedPaymentMethod = "Cash";
                OnPropertyChanged(nameof(GrandTotal));

                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                    message += $"\n\nInner: {ex.InnerException.Message}";
                if (ex.InnerException?.InnerException != null)
                    message += $"\n\nDetail: {ex.InnerException.InnerException.Message}";

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}