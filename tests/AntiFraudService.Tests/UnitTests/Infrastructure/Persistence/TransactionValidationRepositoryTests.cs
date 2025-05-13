using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using AntiFraudService.Domain.Entities;
using AntiFraudService.Domain.Enums;
using AntiFraudService.Infrastructure.Persistence;
using Xunit;

namespace AntiFraudService.Tests.UnitTests.Infrastructure.Persistence
{
    public class TransactionValidationRepositoryAdapterTests : IDisposable
    {
        private readonly AntiFraudDbContext _dbContext;
        private readonly Mock<ILogger<TransactionValidationRepositoryAdapter>> _mockLogger;
        private readonly TransactionValidationRepositoryAdapter _repository;

        public TransactionValidationRepositoryAdapterTests()
        {
            var options = new DbContextOptionsBuilder<AntiFraudDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AntiFraudDbContext(options);
            _mockLogger = new Mock<ILogger<TransactionValidationRepositoryAdapter>>();
            _repository = new TransactionValidationRepositoryAdapter(_dbContext, _mockLogger.Object);
        }

        [Fact]
        public async Task getByTransactionExternalIdAsync_WhenExists_ReturnsValidation()
        {
            // Arrange
            var transactionExternalId = Guid.NewGuid();
            var validation = new TransactionValidation
            {
                TransactionExternalId = transactionExternalId,
                SourceAccountId = Guid.NewGuid(),
                TransactionAmount = 100,
                ValidationDate = DateTime.UtcNow,
                Result = ValidationResult.Approved,
                RejectionReason = null,
                Notes = "Test validation"
            };

            await _dbContext.TransactionValidations.AddAsync(validation);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.getByTransactionExternalIdAsync(transactionExternalId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transactionExternalId, result.TransactionExternalId);
            Assert.Equal(validation.SourceAccountId, result.SourceAccountId);
            Assert.Equal(validation.TransactionAmount, result.TransactionAmount);
        }

        [Fact]
        public async Task getByTransactionExternalIdAsync_WhenNotExists_ThrowsKeyNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _repository.getByTransactionExternalIdAsync(nonExistentId));
        }

        [Fact]
        public async Task addAsync_AddsNewValidation()
        {
            // Arrange
            var validation = new TransactionValidation
            {
                TransactionExternalId = Guid.NewGuid(),
                SourceAccountId = Guid.NewGuid(),
                TransactionAmount = 100,
                ValidationDate = DateTime.UtcNow,
                Result = ValidationResult.Approved,
                RejectionReason = null,
                Notes = "Test validation"
            };

            // Act
            await _repository.addAsync(validation);

            // Assert
            var savedValidation = await _dbContext.TransactionValidations
                .FirstOrDefaultAsync(v => v.TransactionExternalId == validation.TransactionExternalId);
            
            Assert.NotNull(savedValidation);
            Assert.Equal(validation.SourceAccountId, savedValidation.SourceAccountId);
            Assert.Equal(validation.TransactionAmount, savedValidation.TransactionAmount);
            Assert.Equal(validation.Result, savedValidation.Result);
        }

        [Fact]
        public async Task addAsync_WhenDuplicate_IgnoresSecondValidation()
        {
            // Arrange
            var transactionExternalId = Guid.NewGuid();
            var validation1 = new TransactionValidation
            {
                TransactionExternalId = transactionExternalId,
                SourceAccountId = Guid.NewGuid(),
                TransactionAmount = 100,
                ValidationDate = DateTime.UtcNow,
                Result = ValidationResult.Approved,
                RejectionReason = null,
                Notes = "First validation"
            };

            var validation2 = new TransactionValidation
            {
                TransactionExternalId = transactionExternalId,
                SourceAccountId = validation1.SourceAccountId,
                TransactionAmount = 100,
                ValidationDate = DateTime.UtcNow.AddMinutes(5),
                Result = ValidationResult.Rejected,
                RejectionReason = RejectionReason.ExceedsMaximumAmount,
                Notes = "Second validation"
            };

            // Act
            await _repository.addAsync(validation1);
            await _repository.addAsync(validation2);

            // Assert
            var validations = await _dbContext.TransactionValidations
                .Where(v => v.TransactionExternalId == transactionExternalId)
                .ToListAsync();
            
            Assert.Single(validations);
            Assert.Equal("First validation", validations[0].Notes);
            Assert.Equal(ValidationResult.Approved, validations[0].Result);
        }

        [Fact]
        public async Task getDailyTransactionAmountForAccountAsync_ReturnsDailySum()
        {
            // Arrange
            var sourceAccountId = Guid.NewGuid();
            var today = DateTime.UtcNow.Date;
            
            // Create and save some approved validations for today
            var validation1 = new TransactionValidation
            {
                TransactionExternalId = Guid.NewGuid(),
                SourceAccountId = sourceAccountId,
                TransactionAmount = 100,
                ValidationDate = today.AddHours(9),
                Result = ValidationResult.Approved,
                RejectionReason = null
            };
            
            var validation2 = new TransactionValidation
            {
                TransactionExternalId = Guid.NewGuid(),
                SourceAccountId = sourceAccountId,
                TransactionAmount = 200,
                ValidationDate = today.AddHours(14),
                Result = ValidationResult.Approved,
                RejectionReason = null
            };
            
            // This validation should NOT be counted (rejected)
            var validation3 = new TransactionValidation
            {
                TransactionExternalId = Guid.NewGuid(),
                SourceAccountId = sourceAccountId,
                TransactionAmount = 300,
                ValidationDate = today.AddHours(16),
                Result = ValidationResult.Rejected,
                RejectionReason = RejectionReason.ExceedsMaximumAmount
            };
            
            // This validation should NOT be counted (different day)
            var validation4 = new TransactionValidation
            {
                TransactionExternalId = Guid.NewGuid(),
                SourceAccountId = sourceAccountId,
                TransactionAmount = 400,
                ValidationDate = today.AddDays(-1),
                Result = ValidationResult.Approved,
                RejectionReason = null
            };
            
            // This validation should NOT be counted (different account)
            var validation5 = new TransactionValidation
            {
                TransactionExternalId = Guid.NewGuid(),
                SourceAccountId = Guid.NewGuid(), // Different account
                TransactionAmount = 500,
                ValidationDate = today.AddHours(11),
                Result = ValidationResult.Approved,
                RejectionReason = null
            };
            
            await _dbContext.TransactionValidations.AddRangeAsync(
                validation1, validation2, validation3, validation4, validation5);
            await _dbContext.SaveChangesAsync();
            
            // Act
            var result = await _repository.getDailyTransactionAmountForAccountAsync(sourceAccountId, today);
            
            // Assert
            Assert.Equal(300, result); // 100 + 200, only counting approved transactions for today
        }

        [Fact]
        public async Task getDailyTransactionAmountForAccountAsync_WithNoTransactions_ReturnsZero()
        {
            // Arrange
            var sourceAccountId = Guid.NewGuid();
            var today = DateTime.UtcNow.Date;
            
            // Act
            var result = await _repository.getDailyTransactionAmountForAccountAsync(sourceAccountId, today);
            
            // Assert
            Assert.Equal(0, result);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
} 