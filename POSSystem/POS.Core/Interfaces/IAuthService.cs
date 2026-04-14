using POS.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS.Core.Interfaces
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string username, string password);

        Task<List<User>> GetAllUsersAsync();
        Task<int>        AddUserAsync(string username, string displayName, string password, string role);
        Task             UpdateUserAsync(int id, string username, string displayName, string role, bool isActive);
        Task             ChangePasswordAsync(int id, string newPassword);
        Task             DeleteUserAsync(int id);
    }
}
