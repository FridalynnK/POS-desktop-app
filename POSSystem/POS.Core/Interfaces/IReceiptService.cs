using System;
using System.Threading.Tasks;

namespace POS.Core.Interfaces
{
    public interface IReceiptService
    {
        Task PrintReceiptAsync(int saleId);
        Task PrintDebtReceiptAsync(int saleId, decimal amountPaid, DateTime? dueDate);
    }
}
