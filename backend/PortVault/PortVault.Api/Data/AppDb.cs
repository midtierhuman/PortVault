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
        public DbSet<FileUpload> FileUploads => Set<FileUpload>();

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

            modelBuilder.Entity<FileUpload>()
                .HasIndex(f => new { f.PortfolioId, f.FileHash })
                .IsUnique();

            // Removed complex unique constraints for Transactions as we now use deterministic IDs
        }
    }

}

