using System;
using System.Collections.Generic;

namespace POS.Core.DTOs
{
    public class SaleRequestDto
    {
        public int               CashierId     { get; set; }
        public int?              CustomerId    { get; set; }
        public string            PaymentMethod { get; set; } = "Cash";
        public List<SaleItemDto> Items         { get; set; } = new();

        // Debt support — if AmountPaid < total and CustomerId is set, a debt is created
        public decimal?  AmountPaid { get; set; }   // null = fully paid
        public DateTime? DueDate    { get; set; }   // optional repayment deadline
    }
}
