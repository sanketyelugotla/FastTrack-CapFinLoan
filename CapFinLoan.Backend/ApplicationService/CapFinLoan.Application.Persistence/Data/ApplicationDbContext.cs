using CapFinLoan.Application.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CapFinLoan.Application.Persistence.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<ApplicantProfile> ApplicantProfiles => Set<ApplicantProfile>();
    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();
    public DbSet<ApplicationStatusHistory> ApplicationStatusHistories => Set<ApplicationStatusHistory>();
    public DbSet<WalletAccount> WalletAccounts => Set<WalletAccount>();
    public DbSet<WalletLedgerEntry> WalletLedgerEntries => Set<WalletLedgerEntry>();
    public DbSet<PaymentOrder> PaymentOrders => Set<PaymentOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("core");

        modelBuilder.Entity<ApplicantProfile>(entity =>
        {
            entity.ToTable("ApplicantProfiles");
            entity.HasKey(x => x.ApplicantUserId);

            entity.Property(x => x.FirstName).HasMaxLength(100);
            entity.Property(x => x.LastName).HasMaxLength(100);
            entity.Property(x => x.Gender).HasMaxLength(20);
            entity.Property(x => x.Email).HasMaxLength(150);
            entity.Property(x => x.Phone).HasMaxLength(20);
            entity.Property(x => x.AddressLine1).HasMaxLength(250);
            entity.Property(x => x.AddressLine2).HasMaxLength(250);
            entity.Property(x => x.City).HasMaxLength(100);
            entity.Property(x => x.State).HasMaxLength(100);
            entity.Property(x => x.PostalCode).HasMaxLength(15);
            entity.Property(x => x.EmployerName).HasMaxLength(150);
            entity.Property(x => x.EmploymentType).HasMaxLength(50);
            entity.Property(x => x.MonthlyIncome).HasColumnType("decimal(18,2)");
            entity.Property(x => x.AnnualIncome).HasColumnType("decimal(18,2)");
            entity.Property(x => x.ExistingEmiAmount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<LoanApplication>(entity =>
        {
            entity.ToTable("LoanApplications");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ApplicationNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100);
            entity.Property(x => x.LastName).HasMaxLength(100);
            entity.Property(x => x.Gender).HasMaxLength(20);
            entity.Property(x => x.Email).HasMaxLength(150);
            entity.Property(x => x.Phone).HasMaxLength(20);
            entity.Property(x => x.AddressLine1).HasMaxLength(250);
            entity.Property(x => x.AddressLine2).HasMaxLength(250);
            entity.Property(x => x.City).HasMaxLength(100);
            entity.Property(x => x.State).HasMaxLength(100);
            entity.Property(x => x.PostalCode).HasMaxLength(15);
            entity.Property(x => x.EmployerName).HasMaxLength(150);
            entity.Property(x => x.EmploymentType).HasMaxLength(50);
            entity.Property(x => x.MonthlyIncome).HasColumnType("decimal(18,2)");
            entity.Property(x => x.AnnualIncome).HasColumnType("decimal(18,2)");
            entity.Property(x => x.ExistingEmiAmount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.RequestedAmount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.LoanPurpose).HasMaxLength(200);
            entity.Property(x => x.Remarks).HasMaxLength(1000);

            entity.HasIndex(x => x.ApplicationNumber).IsUnique();
            entity.HasMany(x => x.StatusHistory)
                .WithOne(x => x.LoanApplication)
                .HasForeignKey(x => x.LoanApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApplicationStatusHistory>(entity =>
        {
            entity.ToTable("ApplicationStatusHistories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FromStatus).HasMaxLength(30);
            entity.Property(x => x.ToStatus).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Remarks).HasMaxLength(1000);
        });

        modelBuilder.Entity<WalletAccount>(entity =>
        {
            entity.ToTable("WalletAccounts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OwnerType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Currency).HasMaxLength(10).IsRequired();
            entity.HasIndex(x => new { x.OwnerUserId, x.OwnerType }).IsUnique();
        });

        modelBuilder.Entity<WalletLedgerEntry>(entity =>
        {
            entity.ToTable("WalletLedgerEntries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Direction).HasMaxLength(20).IsRequired();
            entity.Property(x => x.EntryType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Currency).HasMaxLength(10).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.ReferenceId).HasMaxLength(100);
            entity.Property(x => x.CorrelationId).HasMaxLength(100);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Remarks).HasMaxLength(1000);
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();

            entity.HasOne(x => x.WalletAccount)
                .WithMany(x => x.LedgerEntries)
                .HasForeignKey(x => x.WalletAccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PaymentOrder>(entity =>
        {
            entity.ToTable("PaymentOrders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Provider).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ProviderOrderId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ProviderPaymentId).HasMaxLength(100);
            entity.Property(x => x.ProviderSignature).HasMaxLength(200);
            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Currency).HasMaxLength(10).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.ProviderOrderId).IsUnique();
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
        });
    }
}