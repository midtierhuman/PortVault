using Microsoft.EntityFrameworkCore;
using PortVault.Api.Models;

namespace PortVault.Api.Data
{
    public sealed class AppDb : DbContext
    {
        public AppDb(DbContextOptions<AppDb> options) : base(options) { }

        public DbSet<Portfolio> Portfolios => Set<Portfolio>();
        public DbSet<PortfolioDetails> PortfolioDetails => Set<PortfolioDetails>();
        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
    }

}
