using System;
using System.Collections.Generic;
using System.Text;

namespace POS.Core.Entities
{
        public class Sale
        {
            public int Id { get; set; }

            public string Reference { get; set; }

            public DateTime DateUtc { get; set; } = DateTime.UtcNow;

            public decimal Total { get; set; }

            public string PaymentMethod { get; set; }

            // Foreign Keys
            public int? CashierId { get; set; }
            public User Cashier { get; set; }

            public int? CustomerId { get; set; }
            public Customer Customer { get; set; }

            // Navigation
        
        public ICollection<SaleItem> SaleItems { get; set; }= new List<SaleItem>();
    }
 

}
