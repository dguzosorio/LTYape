using System;
using System.Threading.Tasks;
using AntiFraudService.Domain.Enums;
using AntiFraudService.Domain.Models;
using AntiFraudService.Domain.Ports;
using AntiFraudService.Domain.Services;
using AntiFraudService.Domain.Settings;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AntiFraudService.Tests.UnitTests.Domain.Services
{
    public class DailyLimitValidationServiceTests
    {
        private readonly Mock<ITransactionValidationRepositoryPort> _mockRepository;
        private readonly Mock<IOptions<ValidationRuleSettings>> _mockOptions;
        private readonly DailyLimitValidationService _service;

        public DailyLimitValidationServiceTests()
        {
            _mockRepository = new Mock<ITransactionValidationRepositoryPort>();
            _mockOptions = new Mock<IOptions<ValidationRuleSettings>>();
            
            _mockOptions.Setup(o => o.Value).Returns(new ValidationRuleSettings
            {
                MaximumDailyLimit = 20000
            });

            _service = new DailyLimitValidationService(_mockRepository.Object, _mockOptions.Object);
        }

        [Fact]
        public async Task ValidateAsync_WhenDailyAmountIsUnderLimit_ReturnsApproved()
        {
            // Arrange
            var transaction = new TransactionValidationRequest
            {
                TransactionExternalId = Guid.NewGuid(),
                SourceAccountId = Guid.NewGuid(),
                Value = 1000,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.getDailyTransactionAmountForAccountAsync(
                    transaction.SourceAccountId, It.IsAny<DateTime>()))
                .ReturnsAsync(10000);

            // Act
            var result = await _service.ValidateAsync(transaction);

            // Assert
            Assert.Equal(ValidationResult.Approved, result.Result);
            Assert.Null(result.RejectionReason);
        }

        [Fact]
        public async Task ValidateAsync_WhenDailyAmountPlusNewTransactionExceedsLimit_ReturnsRejected()
        {
            // Arrange
            var transaction = new TransactionValidationRequest
            {
                TransactionExternalId = Guid.NewGuid(),
                SourceAccountId = Guid.NewGuid(),
                Value = 15000,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.getDailyTransactionAmountForAccountAsync(
                    transaction.SourceAccountId, It.IsAny<DateTime>()))
                .ReturnsAsync(10000);

            // Act
            var result = await _service.ValidateAsync(transaction);

            // Assert
            Assert.Equal(ValidationResult.Rejected, result.Result);
            Assert.Equal(RejectionReason.DailyLimitExceeded, result.RejectionReason);
            Assert.Contains("daily limit", result.Notes);
        }

        [Fact]
        public async Task ValidateAsync_WhenAlreadyAtTheLimit_RejectsAnyNewTransaction()
        {
            // Arrange
            var transaction = new TransactionValidationRequest
            {
                TransactionExternalId = Guid.NewGuid(),
                SourceAccountId = Guid.NewGuid(),
                Value = 1,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.getDailyTransactionAmountForAccountAsync(
                    transaction.SourceAccountId, It.IsAny<DateTime>()))
                .ReturnsAsync(20000);

            // Act
            var result = await _service.ValidateAsync(transaction);

            // Assert
            Assert.Equal(ValidationResult.Rejected, result.Result);
            Assert.Equal(RejectionReason.DailyLimitExceeded, result.RejectionReason);
        }

        [Fact]
        public async Task ValidateAsync_WhenDailyAmountIsExactlyTheLimit_ApprovesZeroValueTransaction()
        {
            // Arrange
            var transaction = new TransactionValidationRequest
            {
                TransactionExternalId = Guid.NewGuid(),
                SourceAccountId = Guid.NewGuid(),
                Value = 0,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.getDailyTransactionAmountForAccountAsync(
                    transaction.SourceAccountId, It.IsAny<DateTime>()))
                .ReturnsAsync(20000);

            // Act
            var result = await _service.ValidateAsync(transaction);

            // Assert
            Assert.Equal(ValidationResult.Approved, result.Result);
        }

        [Fact]
        public async Task ValidateAsync_WhenNoExistingTransactions_ApprovesWithinLimitTransaction()
        {
            // Arrange
            var transaction = new TransactionValidationRequest
            {
                TransactionExternalId = Guid.NewGuid(),
                SourceAccountId = Guid.NewGuid(),
                Value = 15000,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.getDailyTransactionAmountForAccountAsync(
                    transaction.SourceAccountId, It.IsAny<DateTime>()))
                .ReturnsAsync(0);

            // Act
            var result = await _service.ValidateAsync(transaction);

            // Assert
            Assert.Equal(ValidationResult.Approved, result.Result);
        }
    }
} 