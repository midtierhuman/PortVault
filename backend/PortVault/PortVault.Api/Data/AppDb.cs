using Microsoft.EntityFrameworkCore;
using PortVault.Api.Models;

namespace PortVault.Api.Data
{
    public sealed class AppDb : DbContext
    {
        public AppDb(DbContextOptions<AppDb> options) : base(options) { }

        public DbSet<Portfolio> Portfolios => Set<Portfolio>();
        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Holding> Holdings => Set<Holding>();
    }

}
