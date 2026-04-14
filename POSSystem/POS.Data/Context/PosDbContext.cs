using Microsoft.EntityFrameworkCore;
using POS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace POS.Data.Context
{
    public class PosDbContext : DbContext
    {
        public PosDbContext(DbContextOptions<PosDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerBalance> CustomerBalances { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<User> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sale>()
                .HasIndex(s => s.Reference)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}

