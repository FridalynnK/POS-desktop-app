using Microsoft.EntityFrameworkCore;
using POS.Core.Entities;
using POS.Core.Interfaces;
using POS.Data.Context;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IDbContextFactory<PosDbContext> _factory;

        public CustomerService(IDbContextFactory<PosDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            await using var context = await _factory.CreateDbContextAsync();
            return await context.Customers.ToListAsync();
        }

        public async Task<Customer> GetByIdAsync(int id)
        {
            await using var context = await _factory.CreateDbContextAsync();
            return await context.Customers.FindAsync(id);
        }

        public async Task<int> AddCustomerAsync(Customer customer)
        {
            await using var context = await _factory.CreateDbContextAsync();
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            return customer.Id;
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            await using var context = await _factory.CreateDbContextAsync();
            context.Customers.Update(customer);
            await context.SaveChangesAsync();
        }

        public async Task DeleteCustomerAsync(int id)
        {
            await using var context = await _factory.CreateDbContextAsync();
            var customer = await context.Customers.FindAsync(id);
            if (customer != null)
            {
                context.Customers.Remove(customer);
                await context.SaveChangesAsync();
            }
        }
    }
}
