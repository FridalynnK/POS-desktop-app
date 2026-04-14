using System;
using System.Collections.Generic;
using System.Text;

namespace POS.Core.Entities
{
  
        public class CustomerBalance
        {
            public int Id { get; set; }

            public int CustomerId { get; set; }
            public Customer Customer { get; set; }

            public int SaleId { get; set; }
            public Sale Sale { get; set; }

            public decimal TotalAmount { get; set; }

            public decimal Outstanding { get; set; }

            public string Type { get; set; }  // Debt / Advance / Installment

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            public DateTime? DueDate { get; set; }

            public ICollection<Payment> Payments { get; set; }
        }

}
