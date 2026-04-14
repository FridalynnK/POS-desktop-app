using Microsoft.EntityFrameworkCore;
using POS.Core.Interfaces;
using POS.Data.Context;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Threading.Tasks;

namespace POS.Services.Sales
{
    public class ReceiptService : IReceiptService
    {
        private readonly IDbContextFactory<PosDbContext> _factory;

        public ReceiptService(IDbContextFactory<PosDbContext> factory)
        {
            _factory = factory;
        }

        public async Task PrintReceiptAsync(int saleId)
        {
            await using var context = await _factory.CreateDbContextAsync();

            var sale = await context.Sales
                .Include(s => s.SaleItems).ThenInclude(si => si.Product)
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.Id == saleId)
                ?? throw new Exception("Sale not found");

            Print(BuildReceiptText(sale, amountPaid: null, outstanding: null));
        }

        public async Task PrintDebtReceiptAsync(int saleId, decimal amountPaid, DateTime? dueDate)
        {
            await using var context = await _factory.CreateDbContextAsync();

            var sale = await context.Sales
                .Include(s => s.SaleItems).ThenInclude(si => si.Product)
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.Id == saleId)
                ?? throw new Exception("Sale not found");

            decimal outstanding = sale.Total - amountPaid;
            Print(BuildReceiptText(sale, amountPaid, outstanding, dueDate));
        }

        private string BuildReceiptText(
            dynamic sale,
            decimal? amountPaid,
            decimal? outstanding,
            DateTime? dueDate = null)
        {
            var sb = new StringBuilder();
            bool isDebt = outstanding.HasValue && outstanding.Value > 0;

            sb.AppendLine("        MY BUSINESS NAME");
            sb.AppendLine("        Tel: 6XXXXXXXX");
            sb.AppendLine("----------------------------------");

            if (isDebt)
                sb.AppendLine("         *** DEBT RECEIPT ***");

            sb.AppendLine($"Invoice : {sale.Reference}");
            sb.AppendLine($"Date    : {sale.DateUtc:dd/MM/yyyy HH:mm}");

            if (sale.Customer != null)
                sb.AppendLine($"Customer: {sale.Customer.Name}");

            sb.AppendLine("----------------------------------");

            foreach (var item in sale.SaleItems)
                sb.AppendLine(
                    $"{item.Product.Name,-20} x{item.Quantity}\n" +
                    $"  {item.UnitPrice:N0} x {item.Quantity} = {item.LineTotal:N0}");

            sb.AppendLine("----------------------------------");
            sb.AppendLine($"TOTAL       : XAF {sale.Total:N0}");

            if (isDebt)
            {
                sb.AppendLine($"PAID        : XAF {amountPaid:N0}");
                sb.AppendLine($"OUTSTANDING : XAF {outstanding:N0}");
                if (dueDate.HasValue)
                    sb.AppendLine($"DUE DATE    : {dueDate.Value:dd/MM/yyyy}");
            }

            sb.AppendLine("----------------------------------");
            sb.AppendLine(isDebt
                ? "  Please settle your balance soon."
                : "    Thank you for shopping!");

            return sb.ToString();
        }

        private void Print(string text)
        {
            var printDoc = new PrintDocument();
            printDoc.PrintPage += (_, e) =>
                e.Graphics.DrawString(
                    text,
                    new Font("Consolas", 10),
                    Brushes.Black,
                    new RectangleF(0, 0, 300, 800));
            printDoc.Print();
        }
    }
}
