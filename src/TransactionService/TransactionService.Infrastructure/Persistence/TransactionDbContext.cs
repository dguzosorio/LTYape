using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;

namespace TransactionService.Infrastructure.Persistence
{
    /// <summary>
    /// Entity Framework database context for transaction data
    /// </summary>
    public class TransactionDbContext : DbContext
    {
        /// <summary>
        /// Database set of transactions
        /// </summary>
        public DbSet<Transaction> Transactions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionDbContext"/> class
        /// </summary>
        /// <param name="options">The database context options</param>
        public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configures the model that was discovered by convention from the entity types
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transactions");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.TransactionExternalId)
                    .IsRequired();

                entity.Property(e => e.SourceAccountId)
                    .IsRequired();

                entity.Property(e => e.TargetAccountId)
                    .IsRequired();

                entity.Property(e => e.TransferTypeId)
                    .IsRequired();

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasConversion(
                        v => v.ToString(),
                        v => (TransactionStatus)Enum.Parse(typeof(TransactionStatus), v));

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt);

                entity.HasIndex(e => e.TransactionExternalId)
                    .IsUnique();

                entity.HasIndex(e => new { e.SourceAccountId, e.CreatedAt });
            });
        }
    }
} 