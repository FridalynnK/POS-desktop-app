using Microsoft.EntityFrameworkCore;
using POS.Core.DTOs;
using POS.Core.Entities;
using POS.Core.Interfaces;
using POS.Data.Context;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace POS.Services.Sales
{
    public class SaleService : ISaleService
    {
        private readonly IDbContextFactory<PosDbContext> _factory;
        private readonly IDebtService _debtService;

        public SaleService(IDbContextFactory<PosDbContext> factory, IDebtService debtService)
        {
            _factory     = factory;
            _debtService = debtService;
        }

        public async Task<int> CreateSaleAsync(SaleRequestDto request)
        {
            await using var context = await _factory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ Load products
                var productIds = request.Items.Select(i => i.ProductId).ToList();
                var products   = await context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                if (products.Count != request.Items.Count)
                    throw new Exception("One or more products not found.");

                decimal totalAmount = 0;

                // 2️⃣ Validate stock + calculate total
                foreach (var item in request.Items)
                {
                    var product = products.First(p => p.Id == item.ProductId);

                    if (product.Quantity < item.Quantity)
                        throw new Exception($"Insufficient stock for {product.Name}");

                    totalAmount += product.SellingPrice * item.Quantity;
                }

                // 3️⃣ Create Sale
                var sale = new Sale
                {
                    Reference     = GenerateReference(),
                    DateUtc       = DateTime.UtcNow,
                    Total         = totalAmount,
                    PaymentMethod = request.PaymentMethod,
                    CashierId     = request.CashierId,
                    CustomerId    = request.CustomerId
                };

                context.Sales.Add(sale);
                await context.SaveChangesAsync();

                // 4️⃣ Create SaleItems + Deduct Stock
                foreach (var item in request.Items)
                {
                    var product = products.First(p => p.Id == item.ProductId);

                    context.SaleItems.Add(new SaleItem
                    {
                        SaleId    = sale.Id,
                        ProductId = product.Id,
                        Quantity  = item.Quantity,
                        UnitPrice = product.SellingPrice,
                        LineTotal = product.SellingPrice * item.Quantity
                    });

                    product.Quantity -= item.Quantity;
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 5️⃣ Handle debt — uses its own factory context inside DebtService
                if (request.CustomerId.HasValue &&
                    request.AmountPaid.HasValue &&
                    request.AmountPaid.Value < totalAmount)
                {
                    await _debtService.CreateDebtAsync(
                        saleId:     sale.Id,
                        customerId: request.CustomerId.Value,
                        amountPaid: request.AmountPaid.Value,
                        cashierId:  request.CashierId,
                        dueDate:    request.DueDate);
                }

                return sale.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private string GenerateReference()
            => $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}";
    }
}
