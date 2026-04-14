using POS.Core.Entities;
using POS.Core.Interfaces;
using POS.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace POS.UI.ViewModels
{
    public class ProductViewModel : INotifyPropertyChanged
    {
        private readonly IProductService _productService;

        public event PropertyChangedEventHandler? PropertyChanged;

        // ── Collections & selection ──────────────────────────────────────────
        private ObservableCollection<Product> _products = new();
        public ObservableCollection<Product> Products
        {
            get => _products;
            set { _products = value; OnPropertyChanged(); }
        }

        private Product? _selectedProduct;
        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set { _selectedProduct = value; OnPropertyChanged(); }
        }

        // ── UI state ─────────────────────────────────────────────────────────
        private bool _isReadOnlyMode = true;
        public bool IsReadOnlyMode
        {
            get => _isReadOnlyMode;
            set { _isReadOnlyMode = value; OnPropertyChanged(); }
        }

        private bool _isAdding = false;

        // ── Commands ─────────────────────────────────────────────────────────
        public ICommand AddCommand { get; }
        public ICommand ModifyCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }

        // ── Constructor ──────────────────────────────────────────────────────
        public ProductViewModel(IProductService productService)
        {
            _productService = productService;

            AddCommand = new RelayCommand(OnAdd);
            ModifyCommand = new RelayCommand(OnModify,
                                () => SelectedProduct != null);
            DeleteCommand = new RelayCommand(async () => await OnDeleteAsync(),
                                () => SelectedProduct != null && IsReadOnlyMode);
            SaveCommand = new RelayCommand(async () => await OnSaveAsync(),
                                () => !IsReadOnlyMode);
        }

        // ── Data loading ─────────────────────────────────────────────────────
        public async Task LoadProductsAsync()
        {
            var products = await _productService.GetAllProductsAsync();
            Products.Clear();
            foreach (var p in products)
                Products.Add(p);
        }

        // ── Add ──────────────────────────────────────────────────────────────
        private void OnAdd()
        {
            var newProduct = new Product
            {
                AddedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            Products.Add(newProduct);
            SelectedProduct = newProduct;
            IsReadOnlyMode = false;
            _isAdding = true;
        }

        // ── Modify ───────────────────────────────────────────────────────────
        private void OnModify()
        {
            if (SelectedProduct == null) return;
            IsReadOnlyMode = false;
            _isAdding = false;
        }

        // ── Delete ───────────────────────────────────────────────────────────
        private async Task OnDeleteAsync()
        {
            if (SelectedProduct == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{SelectedProduct.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            if (SelectedProduct.Id == 0)
            {
                Products.Remove(SelectedProduct);
                SelectedProduct = null;
                return;
            }

            await _productService.DeleteProductAsync(SelectedProduct.Id);
            Products.Remove(SelectedProduct);
            SelectedProduct = null;
        }

        // ── Save ─────────────────────────────────────────────────────────────
        private async Task OnSaveAsync()
        {
            if (SelectedProduct == null) return;

            try
            {
                if (_isAdding)
                    await _productService.AddProductAsync(SelectedProduct);
                else
                    await _productService.UpdateProductAsync(SelectedProduct);

                IsReadOnlyMode = true;
                _isAdding = false;

                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save failed: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    // ← embedded RelayCommand class DELETED
}