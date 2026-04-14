using Microsoft.EntityFrameworkCore;
using POS.Core.Entities;
using POS.Core.Interfaces;
using POS.Data.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IDbContextFactory<PosDbContext> _factory;

        public ProductService(IDbContextFactory<PosDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            await using var context = await _factory.CreateDbContextAsync();
            return await context.Products.ToListAsync();
        }

        public async Task AddProductAsync(Product product)
        {
            await using var context = await _factory.CreateDbContextAsync();
            product.AddedDate = DateTime.UtcNow;
            product.UpdatedDate = DateTime.UtcNow;
            context.Products.Add(product);
            await context.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(Product product)
        {
            await using var context = await _factory.CreateDbContextAsync();
            product.UpdatedDate = DateTime.UtcNow;
            context.Products.Update(product);
            await context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            await using var context = await _factory.CreateDbContextAsync();
            var product = await context.Products.FindAsync(id);
            if (product != null)
            {
                context.Products.Remove(product);
                await context.SaveChangesAsync();
            }
        }
    }
}
