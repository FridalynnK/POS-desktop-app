using POS.Core.Entities;
using POS.Data.Context;
using System.Linq;

namespace POS.Data.Seed
{
    public static class UserSeeder
    {
        /// <summary>
        /// Call once at startup (e.g. in App.xaml.cs after building the provider).
        /// Creates a default Admin account if no users exist.
        /// 
        /// Default credentials:  admin / admin123
        /// ⚠️  Change the password immediately after first login.
        /// </summary>
        public static void SeedDefaultAdmin(PosDbContext context)
        {
            if (context.Users.Any()) return;

            context.Users.Add(new User
            {
                Username     = "admin",
                DisplayName  = "Administrator",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role         = "Admin",
                IsActive     = true
            });

            context.SaveChanges();
        }
    }
}
