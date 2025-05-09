using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AntiFraudService.Domain.Entities;
using AntiFraudService.Domain.Enums;
using AntiFraudService.Domain.Models;
using AntiFraudService.Domain.Repositories;
using AntiFraudService.Domain.Services;
using Moq;
using Xunit;

namespace AntiFraudService.Tests.UnitTests.Domain.Services
{
    public class AntiFraudDomainServiceTests
    {
        private readonly Mock<ITransactionValidationRepository> _mockRepository;
        private readonly Mock<IValidationRuleService> _mockRuleService1;
        private readonly Mock<IValidationRuleService> _mockRuleService2;
        private readonly AntiFraudDomainService _antiFraudDomainService;
        private readonly Guid _transactionExternalId = Guid.NewGuid();
        private readonly Guid _sourceAccountId = Guid.NewGuid();

        public AntiFraudDomainServiceTests()
        {
            _mockRepository = new Mock<ITransactionValidationRepository>();
            _mockRuleService1 = new Mock<IValidationRuleService>();
            _mockRuleService2 = new Mock<IValidationRuleService>();

            var validationRules = new List<IValidationRuleService>
            {
                _mockRuleService1.Object,
                _mockRuleService2.Object
            };

            _antiFraudDomainService = new AntiFraudDomainService(
                validationRules,
                _mockRepository.Object);
        }

        [Fact]
        public async Task ValidateTransactionAsync_WhenAllRulesApprove_ShouldSaveApprovedValidation()
        {
            // Arrange
            var transactionData = new TransactionData
            {
                TransactionExternalId = _transactionExternalId,
                SourceAccountId = _sourceAccountId,
                Value = 1000,
                CreatedAt = DateTime.UtcNow
            };

            // Configurar las reglas para aprobar la transacción
            _mockRuleService1.Setup(r => r.ValidateAsync(It.IsAny<TransactionData>()))
                .ReturnsAsync(ValidationResponse.CreateApproved(_transactionExternalId));

            _mockRuleService2.Setup(r => r.ValidateAsync(It.IsAny<TransactionData>()))
                .ReturnsAsync(ValidationResponse.CreateApproved(_transactionExternalId));

            TransactionValidation capturedValidation = null;
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<TransactionValidation>()))
                .Callback<TransactionValidation>(v => capturedValidation = v)
                .Returns(Task.CompletedTask);

            // Act
            await _antiFraudDomainService.ValidateTransactionAsync(transactionData);

            // Assert
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<TransactionValidation>()), Times.Once);
            
            Assert.NotNull(capturedValidation);
            Assert.Equal(_transactionExternalId, capturedValidation.TransactionExternalId);
            Assert.Equal(_sourceAccountId, capturedValidation.SourceAccountId);
            Assert.Equal(1000, capturedValidation.TransactionAmount);
            Assert.Equal(ValidationResult.Approved, capturedValidation.Result);
            Assert.Equal(RejectionReason.None, capturedValidation.RejectionReason);
        }

        [Fact]
        public async Task ValidateTransactionAsync_WhenOneRuleRejects_ShouldSaveRejectedValidation()
        {
            // Arrange
            var transactionData = new TransactionData
            {
                TransactionExternalId = _transactionExternalId,
                SourceAccountId = _sourceAccountId,
                Value = 3000,
                CreatedAt = DateTime.UtcNow
            };

            // Configurar la primera regla para aprobar
            _mockRuleService1.Setup(r => r.ValidateAsync(It.IsAny<TransactionData>()))
                .ReturnsAsync(ValidationResponse.CreateApproved(_transactionExternalId));

            // Configurar la segunda regla para rechazar
            _mockRuleService2.Setup(r => r.ValidateAsync(It.IsAny<TransactionData>()))
                .ReturnsAsync(ValidationResponse.CreateRejected(
                    _transactionExternalId,
                    RejectionReason.ExceedsMaximumAmount,
                    "Monto excede el máximo permitido"));

            TransactionValidation capturedValidation = null;
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<TransactionValidation>()))
                .Callback<TransactionValidation>(v => capturedValidation = v)
                .Returns(Task.CompletedTask);

            // Act
            await _antiFraudDomainService.ValidateTransactionAsync(transactionData);

            // Assert
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<TransactionValidation>()), Times.Once);
            
            Assert.NotNull(capturedValidation);
            Assert.Equal(_transactionExternalId, capturedValidation.TransactionExternalId);
            Assert.Equal(_sourceAccountId, capturedValidation.SourceAccountId);
            Assert.Equal(3000, capturedValidation.TransactionAmount);
            Assert.Equal(ValidationResult.Rejected, capturedValidation.Result);
            Assert.Equal(RejectionReason.ExceedsMaximumAmount, capturedValidation.RejectionReason);
            Assert.Contains("Monto excede", capturedValidation.Notes);
        }

        [Fact]
        public async Task ValidateTransactionAsync_WhenBothRulesReject_ShouldSaveFirstRejection()
        {
            // Arrange
            var transactionData = new TransactionData
            {
                TransactionExternalId = _transactionExternalId,
                SourceAccountId = _sourceAccountId,
                Value = 5000,
                CreatedAt = DateTime.UtcNow
            };

            // Configurar la primera regla para rechazar
            _mockRuleService1.Setup(r => r.ValidateAsync(It.IsAny<TransactionData>()))
                .ReturnsAsync(ValidationResponse.CreateRejected(
                    _transactionExternalId,
                    RejectionReason.ExceedsMaximumAmount,
                    "Monto excede el máximo permitido"));

            // Configurar la segunda regla para rechazar también
            _mockRuleService2.Setup(r => r.ValidateAsync(It.IsAny<TransactionData>()))
                .ReturnsAsync(ValidationResponse.CreateRejected(
                    _transactionExternalId,
                    RejectionReason.ExceedsDailyLimit,
                    "Excede el límite diario"));

            TransactionValidation capturedValidation = null;
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<TransactionValidation>()))
                .Callback<TransactionValidation>(v => capturedValidation = v)
                .Returns(Task.CompletedTask);

            // Act
            await _antiFraudDomainService.ValidateTransactionAsync(transactionData);

            // Assert
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<TransactionValidation>()), Times.Once);
            
            Assert.NotNull(capturedValidation);
            Assert.Equal(_transactionExternalId, capturedValidation.TransactionExternalId);
            Assert.Equal(_sourceAccountId, capturedValidation.SourceAccountId);
            Assert.Equal(5000, capturedValidation.TransactionAmount);
            Assert.Equal(ValidationResult.Rejected, capturedValidation.Result);
            
            // Debe tomar la primera regla que rechazó (ExceedsMaximumAmount)
            Assert.Equal(RejectionReason.ExceedsMaximumAmount, capturedValidation.RejectionReason);
            Assert.Contains("Monto excede", capturedValidation.Notes);
        }

        [Fact]
        public async Task ValidateTransactionAsync_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var transactionData = new TransactionData
            {
                TransactionExternalId = _transactionExternalId,
                SourceAccountId = _sourceAccountId,
                Value = 1000,
                CreatedAt = DateTime.UtcNow
            };

            // Configurar las reglas para aprobar la transacción
            _mockRuleService1.Setup(r => r.ValidateAsync(It.IsAny<TransactionData>()))
                .ReturnsAsync(ValidationResponse.CreateApproved(_transactionExternalId));

            _mockRuleService2.Setup(r => r.ValidateAsync(It.IsAny<TransactionData>()))
                .ReturnsAsync(ValidationResponse.CreateApproved(_transactionExternalId));

            // Configurar el repositorio para lanzar una excepción
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<TransactionValidation>()))
                .ThrowsAsync(new InvalidOperationException("Error de base de datos simulado"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _antiFraudDomainService.ValidateTransactionAsync(transactionData));
        }
    }
} 