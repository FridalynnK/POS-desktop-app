using POS.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS.Core.Interfaces
{
    public interface IDebtService
    {
        /// <summary>
        /// Creates a debt record in CustomerBalances after a partial/unpaid sale.
        /// amountPaid can be 0 (full debt) or partial.
        /// </summary>
        Task<int> CreateDebtAsync(int saleId, int customerId, decimal amountPaid, int cashierId, DateTime? dueDate = null);

        /// <summary>
        /// Records a repayment against an existing CustomerBalance.
        /// </summary>
        Task RecordPaymentAsync(int customerBalanceId, decimal amount, string paymentMethod, int cashierId, string? notes = null);

        /// <summary>
        /// All outstanding debts, optionally filtered by customer.
        /// </summary>
        Task<List<CustomerBalance>> GetOutstandingDebtsAsync(int? customerId = null);

        /// <summary>
        /// All registered customers (only these can have debts).
        /// </summary>
        Task<List<Customer>> GetRegisteredCustomersAsync();
    }
}
