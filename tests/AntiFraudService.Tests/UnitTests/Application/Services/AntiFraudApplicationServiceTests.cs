using System;
using System.Threading.Tasks;
using AntiFraudService.Application.DTOs;
using AntiFraudService.Application.Services;
using AntiFraudService.Domain.Entities;
using AntiFraudService.Domain.Enums;
using AntiFraudService.Domain.Models;
using AntiFraudService.Domain.Repositories;
using AntiFraudService.Domain.Services;
using Moq;
using Xunit;

namespace AntiFraudService.Tests.UnitTests.Application.Services
{
    public class AntiFraudApplicationServiceTests
    {
        private readonly Mock<IAntiFraudDomainService> _mockAntiFraudDomainService;
        private readonly Mock<ITransactionValidationRepository> _mockRepository;
        private readonly AntiFraudApplicationService _antiFraudApplicationService;
        private readonly Guid _transactionExternalId = Guid.NewGuid();
        private readonly Guid _sourceAccountId = Guid.NewGuid();
        private readonly Guid _targetAccountId = Guid.NewGuid();

        public AntiFraudApplicationServiceTests()
        {
            _mockAntiFraudDomainService = new Mock<IAntiFraudDomainService>();
            _mockRepository = new Mock<ITransactionValidationRepository>();
            
            _antiFraudApplicationService = new AntiFraudApplicationService(
                _mockAntiFraudDomainService.Object,
                _mockRepository.Object);
        }

        [Fact]
        public async Task ProcessTransactionValidationRequestAsync_ShouldCallDomainService()
        {
            // Arrange
            var request = new TransactionValidationRequest
            {
                TransactionExternalId = _transactionExternalId,
                SourceAccountId = _sourceAccountId,
                TargetAccountId = _targetAccountId,
                TransferTypeId = 1,
                Value = 1000,
                CreatedAt = DateTime.UtcNow
            };
            
            _mockAntiFraudDomainService.Setup(s => 
                s.ValidateTransactionAsync(It.IsAny<TransactionData>()))
                .Returns(Task.CompletedTask);
            
            // Act
            await _antiFraudApplicationService.ProcessTransactionValidationRequestAsync(request);
            
            // Assert
            _mockAntiFraudDomainService.Verify(s => 
                s.ValidateTransactionAsync(It.Is<TransactionData>(data => 
                    data.TransactionExternalId == _transactionExternalId &&
                    data.SourceAccountId == _sourceAccountId &&
                    data.TargetAccountId == _targetAccountId &&
                    data.TransferTypeId == 1 &&
                    data.Value == 1000)),
                Times.Once);
        }

        [Fact]
        public async Task ValidateTransactionAsync_WhenValidationExists_ShouldReturnValidation()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var expectedValidation = new TransactionValidation(
                _transactionExternalId,
                _sourceAccountId,
                1000,
                ValidationResult.Approved);
            
            _mockAntiFraudDomainService.Setup(s => 
                s.ValidateTransactionAsync(It.IsAny<TransactionData>()))
                .Returns(Task.CompletedTask);
            
            _mockRepository.Setup(r => 
                r.GetByTransactionExternalIdAsync(_transactionExternalId))
                .ReturnsAsync(expectedValidation);
            
            // Act
            var result = await _antiFraudApplicationService.ValidateTransactionAsync(
                _transactionExternalId,
                _sourceAccountId,
                1000,
                now);
            
            // Assert
            Assert.Same(expectedValidation, result);
            
            _mockAntiFraudDomainService.Verify(s => 
                s.ValidateTransactionAsync(It.Is<TransactionData>(data => 
                    data.TransactionExternalId == _transactionExternalId &&
                    data.SourceAccountId == _sourceAccountId &&
                    data.Value == 1000 &&
                    data.CreatedAt == now)),
                Times.Once);
        }

        [Fact]
        public async Task ValidateTransactionAsync_WhenValidationNotFound_ShouldCreateDefaultRejected()
        {
            // Arrange
            var now = DateTime.UtcNow;
            
            _mockAntiFraudDomainService.Setup(s => 
                s.ValidateTransactionAsync(It.IsAny<TransactionData>()))
                .Returns(Task.CompletedTask);
            
            // Devolver null para simular que no se encuentra la validación
            _mockRepository.Setup(r => 
                r.GetByTransactionExternalIdAsync(_transactionExternalId))
                .ReturnsAsync((TransactionValidation)null);
            
            TransactionValidation capturedValidation = null;
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<TransactionValidation>()))
                .Callback<TransactionValidation>(v => capturedValidation = v)
                .Returns(Task.CompletedTask);
            
            // Act
            var result = await _antiFraudApplicationService.ValidateTransactionAsync(
                _transactionExternalId,
                _sourceAccountId,
                1000,
                now);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(_transactionExternalId, result.TransactionExternalId);
            Assert.Equal(_sourceAccountId, result.SourceAccountId);
            Assert.Equal(1000, result.TransactionAmount);
            Assert.Equal(ValidationResult.Rejected, result.Result);
            Assert.Equal(RejectionReason.Other, result.RejectionReason);
            Assert.Contains("Error processing validation", result.Notes);
            
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<TransactionValidation>()), Times.Once);
        }

        [Fact]
        public async Task ValidateTransactionAsync_WhenDomainServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var now = DateTime.UtcNow;
            
            _mockAntiFraudDomainService.Setup(s => 
                s.ValidateTransactionAsync(It.IsAny<TransactionData>()))
                .ThrowsAsync(new InvalidOperationException("Error en la validación"));
            
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _antiFraudApplicationService.ValidateTransactionAsync(
                    _transactionExternalId,
                    _sourceAccountId,
                    1000,
                    now));
        }
    }
} 