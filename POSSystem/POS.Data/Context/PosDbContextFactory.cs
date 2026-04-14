using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace POS.Data.Context
{
    public class PosDbContextFactory
        : IDesignTimeDbContextFactory<PosDbContext>
    {
        public PosDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PosDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=DESKTOP-EMHUU2O\\SQLEXPRESS; Database=POSSystemDB;Trusted_Connection=True;TrustServerCertificate=True;"
            );

            return new PosDbContext(optionsBuilder.Options);
        }
    }
}

