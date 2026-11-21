using Microsoft.EntityFrameworkCore;
using WarDudeBankApiV3.Models;

namespace WarDudeBankApiV3.Data;

public class BankDbContext : DbContext
{
    public BankDbContext(DbContextOptions<BankDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.Property(a => a.Owner).HasMaxLength(100);

            entity.Property(a => a.AccountType).HasMaxLength(20);

            entity.Property(a => a.Balance).HasPrecision(18, 2);
        });


        modelBuilder.Entity<BankTransaction>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Type)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(t => t.Amount)
                .HasPrecision(18, 2);

            entity.Property(t => t.BalanceAfter)
                .HasPrecision(18, 2);

            entity.HasOne(t => t.Account)
                .WithMany()
                .HasForeignKey(t => t.AccountId);
        });
    }

}
