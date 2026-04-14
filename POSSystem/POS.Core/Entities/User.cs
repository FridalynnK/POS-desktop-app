using System;
using System.Collections.Generic;
using System.Text;

namespace POS.Core.Entities
{

        public class User
        {
            public int Id { get; set; }

            public string Username { get; set; }

            public string DisplayName { get; set; }

            public string PasswordHash { get; set; }

            public string Role { get; set; }   // Admin or Cashier

            public bool IsActive { get; set; } = true;

            // Navigation
            public ICollection<Sale> Sales { get; set; }
            public ICollection<Payment> Payments { get; set; }
        }
    }


