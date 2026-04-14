using Microsoft.EntityFrameworkCore;
using POS.Core.Entities;
using POS.Data.Context;
using POS.Services.Auth;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace POS.UI.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly IDbContextFactory<PosDbContext> _factory;
        private readonly SessionContext _session;
        private readonly DispatcherTimer _clockTimer;

        // ── Clock ─────────────────────────────────────────────────────
        private string _currentTime = string.Empty;
        private string _currentDate = string.Empty;

        public string CurrentTime
        {
            get => _currentTime;
            set { _currentTime = value; OnPropertyChanged(); }
        }

        public string CurrentDate
        {
            get => _currentDate;
            set { _currentDate = value; OnPropertyChanged(); }
        }

        // ── Business / User ───────────────────────────────────────────
        public string BusinessName => "All In One POS";

        private string _welcomeMessage = string.Empty;
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set { _welcomeMessage = value; OnPropertyChanged(); }
        }

        // ── Summary cards ─────────────────────────────────────────────
        private int _totalProducts;
        public int TotalProducts
        {
            get => _totalProducts;
            set { _totalProducts = value; OnPropertyChanged(); }
        }

        private int _lowStockCount;
        public int LowStockCount
        {
            get => _lowStockCount;
            set { _lowStockCount = value; OnPropertyChanged(); }
        }

        private int _todaysSalesCount;
        public int TodaysSalesCount
        {
            get => _todaysSalesCount;
            set { _todaysSalesCount = value; OnPropertyChanged(); }
        }

        private decimal _todaysRevenue;
        public decimal TodaysRevenue
        {
            get => _todaysRevenue;
            set { _todaysRevenue = value; OnPropertyChanged(); }
        }

        private int _todaysNewProducts;
        public int TodaysNewProducts
        {
            get => _todaysNewProducts;
            set { _todaysNewProducts = value; OnPropertyChanged(); }
        }

        // ── Collections ───────────────────────────────────────────────
        public ObservableCollection<LowStockItem> LowStockItems       { get; } = new();
        public ObservableCollection<Product>      TodaysAddedProducts { get; } = new();

        public DashboardViewModel(IDbContextFactory<PosDbContext> factory, SessionContext session)
        {
            _factory = factory;
            _session = session;

            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (_, _) => TickClock();
            _clockTimer.Start();
            TickClock();

            WelcomeMessage = $"Welcome back, {_session.CurrentUser?.DisplayName ?? "User"}";
        }

        private void TickClock()
        {
            var now = DateTime.Now;
            CurrentTime = now.ToString("hh:mm:ss tt");
            CurrentDate = now.ToString("dddd, MMMM dd, yyyy");
        }

        public async Task LoadAsync()
        {
            await using var context = await _factory.CreateDbContextAsync();

            var today = DateTime.UtcNow.Date;

            TotalProducts = await context.Products.CountAsync(p => p.IsActive);

            var lowStock = await context.Products
                .Where(p => p.IsActive && p.Quantity <= p.ReorderLevel)
                .OrderBy(p => p.Quantity)
                .ToListAsync();

            LowStockCount = lowStock.Count;
            LowStockItems.Clear();
            foreach (var p in lowStock)
                LowStockItems.Add(new LowStockItem
                {
                    Name         = p.Name,
                    SKU          = p.SKU,
                    Quantity     = p.Quantity,
                    ReorderLevel = p.ReorderLevel,
                    Category     = p.Category
                });

            var todaySales = await context.Sales
                .Where(s => s.DateUtc.Date == today)
                .ToListAsync();

            TodaysSalesCount = todaySales.Count;
            TodaysRevenue    = todaySales.Sum(s => s.Total);

            var newProducts = await context.Products
                .Where(p => p.AddedDate.Date == today)
                .OrderByDescending(p => p.AddedDate)
                .ToListAsync();

            TodaysNewProducts = newProducts.Count;
            TodaysAddedProducts.Clear();
            foreach (var p in newProducts)
                TodaysAddedProducts.Add(p);
        }

        public void Dispose() => _clockTimer.Stop();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class LowStockItem
    {
        public string Name         { get; set; } = string.Empty;
        public string SKU          { get; set; } = string.Empty;
        public int    Quantity     { get; set; }
        public int    ReorderLevel { get; set; }
        public string Category     { get; set; } = string.Empty;
        public string StockLabel   => $"{Quantity} / {ReorderLevel}";
        public string Urgency      => Quantity == 0 ? "OUT" : "LOW";
    }
}
