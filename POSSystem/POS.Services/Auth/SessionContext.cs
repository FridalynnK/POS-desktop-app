using POS.Core.Entities;

namespace POS.Services.Auth
{
    /// <summary>
    /// Holds the currently logged-in user for the lifetime of the app session.
    /// Register as a singleton in your DI container.
    /// </summary>
    public class SessionContext
    {
        public User? CurrentUser { get; private set; }

        public bool IsLoggedIn => CurrentUser != null;

        public bool IsAdmin => CurrentUser?.Role == "Admin";
        public bool IsCashier => CurrentUser?.Role == "Cashier";

        public void SetUser(User user)
        {
            CurrentUser = user;
        }

        public void Clear()
        {
            CurrentUser = null;
        }
    }
}
