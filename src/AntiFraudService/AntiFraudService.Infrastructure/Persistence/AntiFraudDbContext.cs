using Microsoft.EntityFrameworkCore;
using AntiFraudService.Domain.Entities;
using AntiFraudService.Domain.Enums;

namespace AntiFraudService.Infrastructure.Persistence
{
    /// <summary>
    /// Entity Framework database context for anti-fraud data
    /// </summary>
    public class AntiFraudDbContext : DbContext
    {
        /// <summary>
        /// Database set of transaction validations
        /// </summary>
        public DbSet<TransactionValidation> TransactionValidations { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AntiFraudDbContext"/> class
        /// </summary>
        /// <param name="options">The database context options</param>
        public AntiFraudDbContext(DbContextOptions<AntiFraudDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configures the model that was discovered by convention from the entity types
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TransactionValidation>(entity =>
            {
                entity.ToTable("TransactionValidations");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.TransactionExternalId)
                    .IsRequired();

                entity.Property(e => e.SourceAccountId)
                    .IsRequired();

                entity.Property(e => e.TransactionAmount)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.ValidationDate)
                    .IsRequired();

                entity.Property(e => e.Result)
                    .IsRequired()
                    .HasConversion(
                        v => v.ToString(),
                        v => (ValidationResult)Enum.Parse(typeof(ValidationResult), v));

                entity.Property(e => e.RejectionReason)
                    .HasConversion(
                        v => v.ToString(),
                        v => (RejectionReason)Enum.Parse(typeof(RejectionReason), v));

                entity.Property(e => e.Notes)
                    .HasMaxLength(500);

                entity.HasIndex(e => e.TransactionExternalId)
                    .IsUnique();

                entity.HasIndex(e => new { e.SourceAccountId, e.ValidationDate });
            });
        }
    }
} 