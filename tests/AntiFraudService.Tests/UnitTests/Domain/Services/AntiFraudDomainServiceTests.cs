using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AntiFraudService.Domain.Entities;
using AntiFraudService.Domain.Enums;
using AntiFraudService.Domain.Models;
using AntiFraudService.Domain.Ports;
using AntiFraudService.Domain.Services;
using Moq;
using Xunit;

namespace AntiFraudService.Tests.UnitTests.Domain.Services
{
    public class AntiFraudDomainServiceTests
    {
        private readonly Mock<ITransactionValidationRepositoryPort> _mockRepository;
        private readonly Mock<ITransactionEventPort> _mockEventPort;
        private readonly Mock<IValidationRuleService> _mockRuleService1;
        private readonly Mock<IValidationRuleService> _mockRuleService2;
        private readonly AntiFraudDomainService _service;

        public AntiFraudDomainServiceTests()
        {
            _mockRepository = new Mock<ITransactionValidationRepositoryPort>();
            _mockEventPort = new Mock<ITransactionEventPort>();
            _mockRuleService1 = new Mock<IValidationRuleService>();
            _mockRuleService2 = new Mock<IValidationRuleService>();

            var validationRules = new List<IValidationRuleService> 
            { 
                _mockRuleService1.Object, 
                _mockRuleService2.Object 
            };

            _service = new AntiFraudDomainService(
                _mockRepository.Object,
                _mockEventPort.Object, 
                validationRules);
        }

        [Fact]
        public async Task ValidateTransactionAsync_WhenAllRulesApprove_ShouldApproveTransaction()
        {
            // Arrange
            var transaction = new TransactionValidationRequest
            {
                TransactionExternalId = Guid.NewGuid(),
                SourceAccountId = Guid.NewGuid(),
                TargetAccountId = Guid.NewGuid(),
                Value = 100,
                CreatedAt = DateTime.UtcNow
            };

            var validationResponse1 = new ValidationResponse
            {
                TransactionExternalId = transaction.TransactionExternalId,
                Result = ValidationResult.Approved,
                RejectionReason = null,
                Notes = "Rule 1 passed"
            };

            var validationResponse2 = new ValidationResponse
            {
                TransactionExternalId = transaction.TransactionExternalId,
                Result = ValidationResult.Approved,
                RejectionReason = null,
                Notes = "Rule 2 passed"
            };

            _mockRuleService1.Setup(r => r.ValidateAsync(It.IsAny<TransactionValidationRequest>()))
                .ReturnsAsync(validationResponse1);
            
            _mockRuleService2.Setup(r => r.ValidateAsync(It.IsAny<TransactionValidationRequest>()))
                .ReturnsAsync(validationResponse2);
            
            _mockRepository.Setup(r => r.addAsync(It.IsAny<TransactionValidation>()))
                .Returns(Task.CompletedTask);
            
            _mockEventPort.Setup(e => e.sendValidationResponseAsync(It.IsAny<ValidationResponse>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.ValidateTransactionAsync(transaction);

            // Assert
            Assert.Equal(ValidationResult.Approved, result.Result);
            Assert.Null(result.RejectionReason);
            Assert.Contains("Rule 1 passed", result.Notes);
            Assert.Contains("Rule 2 passed", result.Notes);
            
            _mockRepository.Verify(r => r.addAsync(It.Is<TransactionValidation>(
                t => t.Result == ValidationResult.Approved)), Times.Once);
            
            _mockEventPort.Verify(e => e.sendValidationResponseAsync(It.Is<ValidationResponse>(
                r => r.Result == ValidationResult.Approved)), Times.Once);
        }

        [Fact]
        public async Task ValidateTransactionAsync_WhenAnyRuleRejects_ShouldRejectTransaction()
        {
            // Arrange
            var transaction = new TransactionValidationRequest
            {
                TransactionExternalId = Guid.NewGuid(),
                SourceAccountId = Guid.NewGuid(),
                TargetAccountId = Guid.NewGuid(),
                Value = 2500,
                CreatedAt = DateTime.UtcNow
            };

            var validationResponse1 = new ValidationResponse
            {
                TransactionExternalId = transaction.TransactionExternalId,
                Result = ValidationResult.Approved,
                RejectionReason = null,
                Notes = "Rule 1 passed"
            };

            var validationResponse2 = new ValidationResponse
            {
                TransactionExternalId = transaction.TransactionExternalId,
                Result = ValidationResult.Rejected,
                RejectionReason = RejectionReason.ExceedsMaximumAmount,
                Notes = "Amount exceeds maximum allowed"
            };

            _mockRuleService1.Setup(r => r.ValidateAsync(It.IsAny<TransactionValidationRequest>()))
                .ReturnsAsync(validationResponse1);
            
            _mockRuleService2.Setup(r => r.ValidateAsync(It.IsAny<TransactionValidationRequest>()))
                .ReturnsAsync(validationResponse2);
            
            _mockRepository.Setup(r => r.addAsync(It.IsAny<TransactionValidation>()))
                .Returns(Task.CompletedTask);
            
            _mockEventPort.Setup(e => e.sendValidationResponseAsync(It.IsAny<ValidationResponse>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.ValidateTransactionAsync(transaction);

            // Assert
            Assert.Equal(ValidationResult.Rejected, result.Result);
            Assert.Equal(RejectionReason.ExceedsMaximumAmount, result.RejectionReason);
            Assert.Contains("Amount exceeds maximum allowed", result.Notes);
            
            _mockRepository.Verify(r => r.addAsync(It.Is<TransactionValidation>(
                t => t.Result == ValidationResult.Rejected)), Times.Once);
            
            _mockEventPort.Verify(e => e.sendValidationResponseAsync(It.Is<ValidationResponse>(
                r => r.Result == ValidationResult.Rejected)), Times.Once);
        }

        [Fact]
        public async Task ValidateTransactionAsync_WhenEventPortFails_ShouldStillSaveValidation()
        {
            // Arrange
            var transaction = new TransactionValidationRequest
            {
                TransactionExternalId = Guid.NewGuid(),
                SourceAccountId = Guid.NewGuid(),
                TargetAccountId = Guid.NewGuid(),
                Value = 100,
                CreatedAt = DateTime.UtcNow
            };

            var validationResponse = new ValidationResponse
            {
                TransactionExternalId = transaction.TransactionExternalId,
                Result = ValidationResult.Approved,
                RejectionReason = null,
                Notes = "All rules passed"
            };

            _mockRuleService1.Setup(r => r.ValidateAsync(It.IsAny<TransactionValidationRequest>()))
                .ReturnsAsync(validationResponse);
            
            _mockRuleService2.Setup(r => r.ValidateAsync(It.IsAny<TransactionValidationRequest>()))
                .ReturnsAsync(validationResponse);
            
            _mockRepository.Setup(r => r.addAsync(It.IsAny<TransactionValidation>()))
                .Returns(Task.CompletedTask);
            
            _mockEventPort.Setup(e => e.sendValidationResponseAsync(It.IsAny<ValidationResponse>()))
                .ThrowsAsync(new Exception("Connection error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.ValidateTransactionAsync(transaction));
            
            // Verify the validation was still saved
            _mockRepository.Verify(r => r.addAsync(It.IsAny<TransactionValidation>()), Times.Once);
        }
    }
} 