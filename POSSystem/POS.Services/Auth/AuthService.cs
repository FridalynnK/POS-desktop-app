using Microsoft.EntityFrameworkCore;
using POS.Core.Entities;
using POS.Core.Interfaces;
using POS.Data.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IDbContextFactory<PosDbContext> _factory;

        public AuthService(IDbContextFactory<PosDbContext> factory)
        {
            _factory = factory;
        }

        // ── Login ─────────────────────────────────────────────────────
        public async Task<User?> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            await using var context = await _factory.CreateDbContextAsync();

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null) return null;

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null;
        }

        // ── Get all ───────────────────────────────────────────────────
        public async Task<List<User>> GetAllUsersAsync()
        {
            await using var context = await _factory.CreateDbContextAsync();
            return await context.Users
                .OrderBy(u => u.Role)
                .ThenBy(u => u.DisplayName)
                .ToListAsync();
        }

        // ── Add ───────────────────────────────────────────────────────
        public async Task<int> AddUserAsync(
            string username, string displayName, string password, string role)
        {
            await using var context = await _factory.CreateDbContextAsync();

            bool exists = await context.Users.AnyAsync(u => u.Username == username);
            if (exists)
                throw new InvalidOperationException($"Username '{username}' is already taken.");

            var user = new User
            {
                Username     = username,
                DisplayName  = displayName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role         = role,
                IsActive     = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user.Id;
        }

        // ── Update ────────────────────────────────────────────────────
        public async Task UpdateUserAsync(
            int id, string username, string displayName, string role, bool isActive)
        {
            await using var context = await _factory.CreateDbContextAsync();

            var user = await context.Users.FindAsync(id)
                ?? throw new InvalidOperationException("User not found.");

            bool duplicate = await context.Users
                .AnyAsync(u => u.Username == username && u.Id != id);
            if (duplicate)
                throw new InvalidOperationException($"Username '{username}' is already taken.");

            user.Username    = username;
            user.DisplayName = displayName;
            user.Role        = role;
            user.IsActive    = isActive;

            await context.SaveChangesAsync();
        }

        // ── Change password ───────────────────────────────────────────
        public async Task ChangePasswordAsync(int id, string newPassword)
        {
            await using var context = await _factory.CreateDbContextAsync();

            var user = await context.Users.FindAsync(id)
                ?? throw new InvalidOperationException("User not found.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await context.SaveChangesAsync();
        }

        // ── Delete ────────────────────────────────────────────────────
        public async Task DeleteUserAsync(int id)
        {
            await using var context = await _factory.CreateDbContextAsync();

            var user = await context.Users.FindAsync(id)
                ?? throw new InvalidOperationException("User not found.");

            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
    }
}
