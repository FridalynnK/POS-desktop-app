using System;
using System.Collections.Generic;
using System.Text;

namespace POS.Core.Entities
{
        public class Payment
        {
            public int Id { get; set; }

            public int CustomerBalanceId { get; set; }
            public CustomerBalance CustomerBalance { get; set; }

            public decimal Amount { get; set; }

            public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

            public string PaymentMethod { get; set; }

            public int? CashierId { get; set; }
            public User Cashier { get; set; }

            public string Notes { get; set; }
        }

}
