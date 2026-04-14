using System;
using System.Collections.Generic;
using System.Text;

namespace POS.Core.Entities
{
        public class Customer
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string Phone { get; set; }

            public string Address { get; set; }

            public string Notes { get; set; }

            // Navigation
            public ICollection<Sale> Sales { get; set; }
            public ICollection<CustomerBalance> CustomerBalances { get; set; }
        }
    

}
