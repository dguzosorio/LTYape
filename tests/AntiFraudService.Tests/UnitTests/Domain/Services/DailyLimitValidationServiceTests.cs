using System;
using System.Threading.Tasks;
using AntiFraudService.Domain.Enums;
using AntiFraudService.Domain.Models;
using AntiFraudService.Domain.Repositories;
using AntiFraudService.Domain.Services;
using Moq;
using Xunit;

namespace AntiFraudService.Tests.UnitTests.Domain.Services
{
    public class DailyLimitValidationServiceTests
    {
        private readonly Mock<ITransactionValidationRepository> _mockRepository;
        private readonly DailyLimitValidationService _dailyLimitValidationService;
        private readonly Guid _transactionExternalId = Guid.NewGuid();
        private readonly Guid _sourceAccountId = Guid.NewGuid();
        private readonly decimal _maximumDailyLimit = 20000; // Mismo valor que en la clase original

        public DailyLimitValidationServiceTests()
        {
            _mockRepository = new Mock<ITransactionValidationRepository>();
            _dailyLimitValidationService = new DailyLimitValidationService(_mockRepository.Object);
        }

        [Fact]
        public async Task ValidateAsync_WhenDailyTotalBelowLimit_ShouldApproveTransaction()
        {
            // Arrange
            var currentDate = DateTime.UtcNow.Date;
            var transactionData = new TransactionData
            {
                TransactionExternalId = _transactionExternalId,
                SourceAccountId = _sourceAccountId,
                Value = 1000,
                CreatedAt = currentDate.AddHours(12) // Mediodía del día actual
            };

            // Configurar el repositorio para devolver un total actual de 5000
            _mockRepository.Setup(repo => 
                repo.GetDailyTransactionAmountForAccountAsync(_sourceAccountId, currentDate))
                .ReturnsAsync(5000);

            // Act
            var result = await _dailyLimitValidationService.ValidateAsync(transactionData);

            // Assert
            Assert.Equal(ValidationResult.Approved, result.Result);
            Assert.Equal(_transactionExternalId, result.TransactionExternalId);
            Assert.Equal(RejectionReason.None, result.RejectionReason);
        }

        [Fact]
        public async Task ValidateAsync_WhenDailyTotalAtLimit_ShouldApproveTransaction()
        {
            // Arrange
            var currentDate = DateTime.UtcNow.Date;
            var transactionData = new TransactionData
            {
                TransactionExternalId = _transactionExternalId,
                SourceAccountId = _sourceAccountId,
                Value = 5000,
                CreatedAt = currentDate.AddHours(12)
            };

            // Configurar el repositorio para devolver un total actual de 15000
            // 15000 + 5000 = 20000 (justo en el límite)
            _mockRepository.Setup(repo => 
                repo.GetDailyTransactionAmountForAccountAsync(_sourceAccountId, currentDate))
                .ReturnsAsync(15000);

            // Act
            var result = await _dailyLimitValidationService.ValidateAsync(transactionData);

            // Assert
            Assert.Equal(ValidationResult.Approved, result.Result);
            Assert.Equal(_transactionExternalId, result.TransactionExternalId);
            Assert.Equal(RejectionReason.None, result.RejectionReason);
        }

        [Fact]
        public async Task ValidateAsync_WhenDailyTotalExceedsLimit_ShouldRejectTransaction()
        {
            // Arrange
            var currentDate = DateTime.UtcNow.Date;
            var transactionData = new TransactionData
            {
                TransactionExternalId = _transactionExternalId,
                SourceAccountId = _sourceAccountId,
                Value = 10000,
                CreatedAt = currentDate.AddHours(12)
            };

            // Configurar el repositorio para devolver un total actual de 15000
            // 15000 + 10000 = 25000 (excede el límite de 20000)
            _mockRepository.Setup(repo => 
                repo.GetDailyTransactionAmountForAccountAsync(_sourceAccountId, currentDate))
                .ReturnsAsync(15000);

            // Act
            var result = await _dailyLimitValidationService.ValidateAsync(transactionData);

            // Assert
            Assert.Equal(ValidationResult.Rejected, result.Result);
            Assert.Equal(_transactionExternalId, result.TransactionExternalId);
            Assert.Equal(RejectionReason.ExceedsDailyLimit, result.RejectionReason);
            Assert.Contains("daily limit", result.Notes);
            Assert.Contains("15000", result.Notes);
            Assert.Contains("10000", result.Notes);
            Assert.Contains("20000", result.Notes);
        }

        [Fact]
        public async Task ValidateAsync_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var currentDate = DateTime.UtcNow.Date;
            var transactionData = new TransactionData
            {
                TransactionExternalId = _transactionExternalId,
                SourceAccountId = _sourceAccountId,
                Value = 1000,
                CreatedAt = currentDate.AddHours(12)
            };

            // Configurar el repositorio para lanzar una excepción
            _mockRepository.Setup(repo => 
                repo.GetDailyTransactionAmountForAccountAsync(_sourceAccountId, currentDate))
                .ThrowsAsync(new InvalidOperationException("Error de base de datos simulado"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _dailyLimitValidationService.ValidateAsync(transactionData));
        }
    }
} 