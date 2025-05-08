using System;
using System.Threading.Tasks;
using AntiFraudService.Domain.Enums;
using AntiFraudService.Domain.Models;
using AntiFraudService.Domain.Services;
using Xunit;

namespace AntiFraudService.Tests.UnitTests.Domain.Services
{
    public class MaximumAmountValidationServiceTests
    {
        private readonly MaximumAmountValidationService _maximumAmountValidationService;
        private readonly Guid _transactionExternalId = Guid.NewGuid();
        private readonly Guid _sourceAccountId = Guid.NewGuid();
        private readonly decimal _maximumAmount = 2000; // Mismo valor que en la clase original

        public MaximumAmountValidationServiceTests()
        {
            _maximumAmountValidationService = new MaximumAmountValidationService();
        }

        [Fact]
        public async Task ValidateAsync_WhenAmountBelowMaximum_ShouldApproveTransaction()
        {
            // Arrange
            var transactionData = new TransactionData
            {
                TransactionExternalId = _transactionExternalId,
                SourceAccountId = _sourceAccountId,
                Value = 1500, // Menor que el límite de 2000
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _maximumAmountValidationService.ValidateAsync(transactionData);

            // Assert
            Assert.Equal(ValidationResult.Approved, result.Result);
            Assert.Equal(_transactionExternalId, result.TransactionExternalId);
            Assert.Equal(RejectionReason.None, result.RejectionReason);
        }

        [Fact]
        public async Task ValidateAsync_WhenAmountAtMaximum_ShouldApproveTransaction()
        {
            // Arrange
            var transactionData = new TransactionData
            {
                TransactionExternalId = _transactionExternalId,
                SourceAccountId = _sourceAccountId,
                Value = 2000, // Igual que el límite
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _maximumAmountValidationService.ValidateAsync(transactionData);

            // Assert
            Assert.Equal(ValidationResult.Approved, result.Result);
            Assert.Equal(_transactionExternalId, result.TransactionExternalId);
            Assert.Equal(RejectionReason.None, result.RejectionReason);
        }

        [Fact]
        public async Task ValidateAsync_WhenAmountExceedsMaximum_ShouldRejectTransaction()
        {
            // Arrange
            var transactionData = new TransactionData
            {
                TransactionExternalId = _transactionExternalId,
                SourceAccountId = _sourceAccountId,
                Value = 2500, // Mayor que el límite de 2000
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _maximumAmountValidationService.ValidateAsync(transactionData);

            // Assert
            Assert.Equal(ValidationResult.Rejected, result.Result);
            Assert.Equal(_transactionExternalId, result.TransactionExternalId);
            Assert.Equal(RejectionReason.ExceedsMaximumAmount, result.RejectionReason);
            Assert.Contains("2500", result.Notes);
            Assert.Contains("2000", result.Notes);
        }

        [Theory]
        [InlineData(0.01)]
        [InlineData(1)]
        [InlineData(1999.99)]
        public async Task ValidateAsync_WithVariousValidAmounts_ShouldApproveTransaction(decimal amount)
        {
            // Arrange
            var transactionData = new TransactionData
            {
                TransactionExternalId = _transactionExternalId,
                SourceAccountId = _sourceAccountId,
                Value = amount,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _maximumAmountValidationService.ValidateAsync(transactionData);

            // Assert
            Assert.Equal(ValidationResult.Approved, result.Result);
        }

        [Theory]
        [InlineData(2000.01)]
        [InlineData(5000)]
        [InlineData(10000)]
        public async Task ValidateAsync_WithVariousInvalidAmounts_ShouldRejectTransaction(decimal amount)
        {
            // Arrange
            var transactionData = new TransactionData
            {
                TransactionExternalId = _transactionExternalId,
                SourceAccountId = _sourceAccountId,
                Value = amount,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _maximumAmountValidationService.ValidateAsync(transactionData);

            // Assert
            Assert.Equal(ValidationResult.Rejected, result.Result);
            Assert.Equal(RejectionReason.ExceedsMaximumAmount, result.RejectionReason);
        }
    }
} 