using Microsoft.EntityFrameworkCore;
using PortVault.Api.Models;

namespace PortVault.Api.Data
{
    public sealed class AppDb : DbContext
    {
        public AppDb(DbContextOptions<AppDb> options) : base(options) { }

        public DbSet<AppUser> Users => Set<AppUser>();
        public DbSet<Portfolio> Portfolios => Set<Portfolio>();
        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Holding> Holdings => Set<Holding>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>()
                .HasIndex(x => x.Username)
                .IsUnique();

            modelBuilder.Entity<AppUser>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<Holding>()
                .HasKey(h => new { h.PortfolioId, h.ISIN });

            modelBuilder.Entity<Portfolio>()
                .HasIndex(p => new { p.UserId, p.Name })
                .IsUnique();

            // Unique constraint for STOCKS (when TradeID exists) - Use ONLY TradeID
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => new { t.PortfolioId, t.TradeID })
                .IsUnique()
                .HasFilter("[TradeID] IS NOT NULL")
                .HasDatabaseName("IX_Transaction_StockUnique");

            // Unique constraint for MUTUAL FUNDS (when TradeID is null)
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => new {
                    t.PortfolioId,
                    t.ISIN,
                    t.TradeDate,
                    t.TradeType,
                    t.Quantity,
                    t.Price
                })
                .IsUnique()
                .HasFilter("[TradeID] IS NULL")
                .HasDatabaseName("IX_Transaction_MFUnique");
        }
    }

}

