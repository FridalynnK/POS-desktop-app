using Microsoft.Extensions.DependencyInjection;
using POS.Services.Auth;
using POS.UI.Views;
using System;
using System.Windows;
using System.Windows.Controls;

namespace POS.UI
{
    public partial class MainWindow : Window
    {
        private readonly SessionContext _session;
        private IServiceScope? _currentScope;  // ← ADD THIS
        public MainWindow(SessionContext session)
        {
            InitializeComponent();
            _session = session;
        }

        public void InitializeForUser()
        {
            var user = _session.CurrentUser!;

            UserNameLabel.Text = user.DisplayName;   // ← was .Content
            RoleLabel.Text     = user.Role;           // ← was .Content

            // Hide nav items based on role
            NavProducts.Visibility = _session.IsAdmin
                ? Visibility.Visible : Visibility.Collapsed;
            NavReports.Visibility = _session.IsAdmin
                ? Visibility.Visible : Visibility.Collapsed;
            NavUsers.Visibility = _session.IsAdmin
                ? Visibility.Visible : Visibility.Collapsed;

            // Start on dashboard
            NavigateTo("Dashboard");
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                NavigateTo(btn.Tag?.ToString() ?? "Dashboard");
        }

        private void NavigateTo(string page)
        {
            // Dispose previous scope if any
            _currentScope?.Dispose();

            // Create a fresh scope → fresh DbContext for each view
            _currentScope = App.ServiceProvider!.CreateScope();

            MainContent.Content = page switch
            {
                "Dashboard" => _currentScope.ServiceProvider.GetRequiredService<AdminDashboardView>(),
                "Sales" => _currentScope.ServiceProvider.GetRequiredService<CashierView>(),
                "Products" => _currentScope.ServiceProvider.GetRequiredService<ProductView>(),
                "Customers" => _currentScope.ServiceProvider.GetRequiredService<CustomerView>(),
                "Debts" => _currentScope.ServiceProvider.GetRequiredService<DebtManagementView>(),
                "Reports" => _currentScope.ServiceProvider.GetRequiredService<AdminDashboardView>(),
                "Users" => App.ServiceProvider!.GetRequiredService<UserManagementView>(),
            };
        }

        private void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            // NavigateTo("Profile"); // wire up later
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            _session.Clear();
            var login = App.ServiceProvider!.GetRequiredService<LoginView>();
            login.Show();
            this.Close();
        }
        protected override void OnClosed(EventArgs e)
        {
            _currentScope?.Dispose();  // ← ADD THIS
            base.OnClosed(e);
        }
    }
}
