using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PortVault.Api.TempModels;

public partial class TempContext : DbContext
{
    public TempContext(DbContextOptions<TempContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Asset> Assets { get; set; }

    public virtual DbSet<Holding> Holdings { get; set; }

    public virtual DbSet<Portfolio> Portfolios { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Isin);

            entity.Property(e => e.Isin).HasColumnName("ISIN");
            entity.Property(e => e.Inav).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MarketPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Nav).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Holding>(entity =>
        {
            entity.HasKey(e => new { e.PortfolioId, e.Isin });

            entity.Property(e => e.Isin).HasColumnName("ISIN");
            entity.Property(e => e.AvgPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Qty).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Portfolio>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.Name }, "IX_Portfolios_UserId_Name").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Current).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Invested).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasIndex(e => new { e.PortfolioId, e.Isin, e.TradeDate, e.TradeType, e.Quantity, e.Price }, "IX_Transaction_MFUnique")
                .IsUnique()
                .HasFilter("([TradeID] IS NULL)");

            entity.HasIndex(e => new { e.PortfolioId, e.TradeId }, "IX_Transaction_StockUnique")
                .IsUnique()
                .HasFilter("([TradeID] IS NOT NULL)");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Isin).HasColumnName("ISIN");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TradeId).HasColumnName("TradeID");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();

            entity.HasIndex(e => e.Username, "IX_Users_Username").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Username).HasMaxLength(64);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
