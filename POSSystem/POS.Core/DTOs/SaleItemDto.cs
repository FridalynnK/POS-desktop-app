using System;
using System.Collections.Generic;
using System.Text;

namespace POS.Core.DTOs
{
    public class SaleItemDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
    }

}
