using POS.Core.Interfaces;
using POS.Services.Auth;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using POS.UI.Commands;  // instead of POS.UI.ViewModels

namespace POS.UI.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly SessionContext _session;

        private string _username = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;
        public string Password { get; set; }

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        // Password is NOT bound via ViewModel for security.
        // It is passed directly from the PasswordBox in code-behind.

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // Raised when login succeeds — UI subscribes to this
        // ✅ fixed
        public event EventHandler<string>? LoginSucceeded;

        public ICommand LoginCommand { get; }

        public LoginViewModel(IAuthService authService, SessionContext session)
        {
            _authService = authService;
            _session = session;
            LoginCommand = new AsyncRelayCommand<string>(ExecuteLoginAsync);
        }

        private async Task ExecuteLoginAsync(string? password)
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Please enter your username and password.";
                return;
            }

            IsLoading = true;

            try
            {
                var user = await _authService.LoginAsync(Username, password);

                if (user == null)
                {
                    ErrorMessage = "Invalid username or password.";
                    return;
                }

                _session.SetUser(user);
                // ✅ fixed
                LoginSucceeded?.Invoke(this, user.Role);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
