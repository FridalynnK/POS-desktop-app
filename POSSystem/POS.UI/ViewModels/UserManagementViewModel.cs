using POS.Core.Entities;
using POS.Core.Interfaces;
using POS.Services.Auth;
using POS.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace POS.UI.ViewModels
{
    public class UserManagementViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService  _authService;
        private readonly SessionContext _session;

        // ── Collections ───────────────────────────────────────────────
        public ObservableCollection<User> Users { get; } = new();

        public List<string> Roles { get; } = new() { "Admin", "Cashier" };

        // ── Selected user ─────────────────────────────────────────────
        private User? _selectedUser;
        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
                if (value != null) LoadFormFrom(value);
                else               ClearForm();
            }
        }

        // ── Form fields ───────────────────────────────────────────────
        private string _username    = string.Empty;
        private string _displayName = string.Empty;
        private string _password    = string.Empty;
        private string _role        = "Cashier";
        private bool   _isActive    = true;
        private bool   _isEditing;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string DisplayName
        {
            get => _displayName;
            set { _displayName = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string Role
        {
            get => _role;
            set { _role = value; OnPropertyChanged(); }
        }

        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormTitle));
                OnPropertyChanged(nameof(ShowPasswordField));
            }
        }

        public string FormTitle       => IsEditing ? "Edit User" : "New User";
        public bool ShowPasswordField => !IsEditing; // only required on add; use Change Password for existing

        // ── Status ────────────────────────────────────────────────────
        private string _statusMessage = string.Empty;
        private bool   _isError;

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public bool IsError
        {
            get => _isError;
            set { _isError = value; OnPropertyChanged(); }
        }

        // ── Commands ──────────────────────────────────────────────────
        public ICommand NewCommand            { get; }
        public ICommand SaveCommand           { get; }
        public ICommand DeleteCommand         { get; }
        public ICommand CancelCommand         { get; }
        public ICommand ChangePasswordCommand { get; }

        public UserManagementViewModel(IAuthService authService, SessionContext session)
        {
            _authService = authService;
            _session     = session;

            NewCommand            = new RelayCommand(OnNew);
            SaveCommand           = new AsyncRelayCommand<object>(_ => OnSaveAsync());
            DeleteCommand         = new AsyncRelayCommand<object>(_ => OnDeleteAsync(),
                                        _ => SelectedUser != null
                                          && SelectedUser.Id != _session.CurrentUser?.Id);
            CancelCommand         = new RelayCommand(OnCancel);
            ChangePasswordCommand = new AsyncRelayCommand<object>(_ => OnChangePasswordAsync(),
                                        _ => SelectedUser != null && IsEditing);
        }

        // ── Load ──────────────────────────────────────────────────────
        public async Task LoadAsync()
        {
            var users = await _authService.GetAllUsersAsync();

            App.Current.Dispatcher.Invoke(() =>
            {
                Users.Clear();
                foreach (var u in users) Users.Add(u);
            });
        }

        // ── New ───────────────────────────────────────────────────────
        private void OnNew()
        {
            _selectedUser = null;
            OnPropertyChanged(nameof(SelectedUser));
            ClearForm();
            IsEditing = false;
        }

        // ── Save (add or update) ──────────────────────────────────────
        private async Task OnSaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Username))
            { ShowStatus("Username is required.", error: true); return; }

            if (string.IsNullOrWhiteSpace(DisplayName))
            { ShowStatus("Display name is required.", error: true); return; }

            if (!IsEditing && string.IsNullOrWhiteSpace(Password))
            { ShowStatus("Password is required for new users.", error: true); return; }

            try
            {
                if (IsEditing && SelectedUser != null)
                {
                    await _authService.UpdateUserAsync(
                        SelectedUser.Id, Username, DisplayName, Role, IsActive);
                    ShowStatus($"'{DisplayName}' updated successfully.", error: false);
                }
                else
                {
                    await _authService.AddUserAsync(Username, DisplayName, Password, Role);
                    ShowStatus($"'{DisplayName}' added successfully.", error: false);
                }

                OnCancel();
                await LoadAsync();
            }
            catch (Exception ex)
            {
                ShowStatus(ex.Message, error: true);
            }
        }

        // ── Delete ────────────────────────────────────────────────────
        private async Task OnDeleteAsync()
        {
            if (SelectedUser == null) return;

            if (SelectedUser.Id == _session.CurrentUser?.Id)
            { ShowStatus("You cannot delete your own account.", error: true); return; }

            var result = MessageBox.Show(
                $"Delete user '{SelectedUser.DisplayName}'? This cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _authService.DeleteUserAsync(SelectedUser.Id);
                ShowStatus("User deleted.", error: false);
                OnCancel();
                await LoadAsync();
            }
            catch (Exception ex)
            {
                ShowStatus(ex.Message, error: true);
            }
        }

        // ── Change password ───────────────────────────────────────────
        private async Task OnChangePasswordAsync()
        {
            if (SelectedUser == null) return;

            if (string.IsNullOrWhiteSpace(Password))
            { ShowStatus("Enter a new password in the password field.", error: true); return; }

            try
            {
                await _authService.ChangePasswordAsync(SelectedUser.Id, Password);
                Password = string.Empty;
                ShowStatus("Password changed successfully.", error: false);
            }
            catch (Exception ex)
            {
                ShowStatus(ex.Message, error: true);
            }
        }

        // ── Cancel / clear ────────────────────────────────────────────
        private void OnCancel()
        {
            _selectedUser = null;
            OnPropertyChanged(nameof(SelectedUser));
            ClearForm();
        }

        // ── Helpers ───────────────────────────────────────────────────
        private void LoadFormFrom(User u)
        {
            Username    = u.Username    ?? string.Empty;
            DisplayName = u.DisplayName ?? string.Empty;
            Password    = string.Empty;
            Role        = u.Role        ?? "Cashier";
            IsActive    = u.IsActive;
            IsEditing   = true;
        }

        private void ClearForm()
        {
            Username    = string.Empty;
            DisplayName = string.Empty;
            Password    = string.Empty;
            Role        = "Cashier";
            IsActive    = true;
            IsEditing   = false;
            StatusMessage = string.Empty;
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
