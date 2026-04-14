using Microsoft.EntityFrameworkCore;
using POS.Core.Entities;
using POS.Core.Interfaces;
using POS.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POS.Services.Payments
{
    public class DebtService : IDebtService
    {
        private readonly IDbContextFactory<PosDbContext> _factory;

        public DebtService(IDbContextFactory<PosDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<int> CreateDebtAsync(
            int saleId,
            int customerId,
            decimal amountPaid,
            int cashierId,
            DateTime? dueDate = null)
        {
            await using var context = await _factory.CreateDbContextAsync();

            var customer = await context.Customers.FindAsync(customerId)
                ?? throw new InvalidOperationException("Customer not found. Only registered customers can have debts.");

            var sale = await context.Sales.FindAsync(saleId)
                ?? throw new InvalidOperationException("Sale not found.");

            var outstanding = sale.Total - amountPaid;

            if (outstanding <= 0)
                throw new InvalidOperationException("No outstanding amount — sale is fully paid.");

            var balance = new CustomerBalance
            {
                CustomerId  = customerId,
                SaleId      = saleId,
                TotalAmount = sale.Total,
                Outstanding = outstanding,
                Type        = "Debt",
                CreatedAt   = DateTime.UtcNow,
                DueDate     = dueDate
            };

            context.CustomerBalances.Add(balance);

            if (amountPaid > 0)
            {
                await context.SaveChangesAsync(); // get balance.Id

                var payment = new Payment
                {
                    CustomerBalanceId = balance.Id,
                    Amount            = amountPaid,
                    PaymentDate       = DateTime.UtcNow,
                    PaymentMethod     = sale.PaymentMethod ?? "Cash",
                    CashierId         = cashierId,
                    Notes             = "Initial partial payment at time of sale"
                };

                context.Payments.Add(payment);
            }

            await context.SaveChangesAsync();

            return balance.Id;
        }

        public async Task RecordPaymentAsync(
            int customerBalanceId,
            decimal amount,
            string paymentMethod,
            int cashierId,
            string? notes = null)
        {
            await using var context = await _factory.CreateDbContextAsync();

            var balance = await context.CustomerBalances.FindAsync(customerBalanceId)
                ?? throw new InvalidOperationException("Debt record not found.");

            if (amount <= 0)
                throw new InvalidOperationException("Payment amount must be greater than zero.");

            if (amount > balance.Outstanding)
                throw new InvalidOperationException(
                    $"Payment ({amount:N0}) exceeds outstanding balance ({balance.Outstanding:N0}).");

            context.Payments.Add(new Payment
            {
                CustomerBalanceId = customerBalanceId,
                Amount            = amount,
                PaymentDate       = DateTime.UtcNow,
                PaymentMethod     = paymentMethod,
                CashierId         = cashierId,
                Notes             = notes
            });

            balance.Outstanding -= amount;

            await context.SaveChangesAsync();
        }

        public async Task<List<CustomerBalance>> GetOutstandingDebtsAsync(int? customerId = null)
        {
            await using var context = await _factory.CreateDbContextAsync();

            var query = context.CustomerBalances
                .Include(b => b.Customer)
                .Include(b => b.Sale)
                .Include(b => b.Payments)
                .Where(b => b.Outstanding > 0 && b.Type == "Debt");

            if (customerId.HasValue)
                query = query.Where(b => b.CustomerId == customerId.Value);

            return await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Customer>> GetRegisteredCustomersAsync()
        {
            await using var context = await _factory.CreateDbContextAsync();

            return await context.Customers
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}
