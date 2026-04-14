using System;
using System.Collections.Generic;
using System.Text;

namespace POS.Core.Entities
{
        public class SaleItem
        {
            public int Id { get; set; }

            public int SaleId { get; set; }
            public Sale Sale { get; set; }

            public int ProductId { get; set; }
            public Product Product { get; set; }

            public decimal UnitPrice { get; set; }

            public int Quantity { get; set; }

            public decimal LineTotal { get; set; }
        }

}
