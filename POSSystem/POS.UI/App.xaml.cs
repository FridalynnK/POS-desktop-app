using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using POS.Core.Interfaces;
using POS.Data.Context;
using POS.Data.Seed;
using POS.Services;
using POS.Services.Auth;
using POS.Services.Payments;
using POS.Services.Products;
using POS.Services.Sales;
using POS.UI.ViewModels;
using POS.UI.Views;
using System;
using System.Windows;

namespace POS.UI
{
    public partial class App : Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            // ── Database ──────────────────────────────────────────────
            // Factory pattern: each service creates its own short-lived context
            // — eliminates the concurrent DbContext crash completely
            services.AddDbContextFactory<PosDbContext>(options =>
                options.UseSqlServer(
                    "Server=DESKTOP-EMHUU2O\\SQLEXPRESS;Database=POSSystemDB;Trusted_Connection=True;TrustServerCertificate=True;"
                ));

            // ── Services ──────────────────────────────────────────────
            services.AddScoped<IAuthService,     AuthService>();
            services.AddScoped<IDebtService,     DebtService>();
            services.AddScoped<ISaleService,     SaleService>();
            services.AddScoped<IReceiptService,  ReceiptService>();
            services.AddScoped<IProductService,  ProductService>();
            services.AddScoped<ICustomerService, CustomerService>();

            services.AddSingleton<SessionContext>();

            // ── ViewModels ────────────────────────────────────────────
            services.AddTransient<LoginViewModel>();
            services.AddTransient<CashierViewModel>();
            services.AddTransient<ProductViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<DebtViewModel>();
            services.AddTransient<CustomerViewModel>();

            // ── Views ─────────────────────────────────────────────────
            services.AddTransient<LoginView>();
            services.AddTransient<CashierView>();
            services.AddTransient<ProductView>();
            services.AddTransient<AdminDashboardView>();
            services.AddTransient<DebtManagementView>();
            services.AddTransient<CustomerView>();
            services.AddTransient<MainWindow>();
            services.AddTransient<UserManagementViewModel>();
            services.AddTransient<UserManagementView>();

            ServiceProvider = services.BuildServiceProvider();

            // ── Seed default admin on first run ───────────────────────
            using (var scope = ServiceProvider.CreateScope())
            {
                var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PosDbContext>>();
                using var db = factory.CreateDbContext();
                UserSeeder.SeedDefaultAdmin(db);
            }

            // ── Launch the Login window ───────────────────────────────
            var loginView = ServiceProvider.GetRequiredService<LoginView>();
            loginView.Show();
        }
    }
}
