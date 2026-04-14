using System;
using System.Collections.Generic;
using System.Text;


    namespace POS.Core.Entities
    {
        public class Product
        {
            public int Id { get; set; }

            public string SKU { get; set; }

            public string Name { get; set; }

            public string Category { get; set; }

            public decimal BuyingPrice { get; set; }

            public decimal SellingPrice { get; set; }

            public int Quantity { get; set; }

            public int ReorderLevel { get; set; }

            public DateTime? ExpiryDate { get; set; }

            public bool IsActive { get; set; } = true;
            public DateTime AddedDate { get; set; }
            public DateTime? UpdatedDate { get; set; }

        // Navigation
        public ICollection<SaleItem> SaleItems { get; set; }
        }
    }


