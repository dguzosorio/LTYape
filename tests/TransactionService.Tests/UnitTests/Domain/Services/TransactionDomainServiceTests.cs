using System;
using System.Threading.Tasks;
using Moq;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Domain.Exceptions;
using TransactionService.Domain.Repositories;
using TransactionService.Domain.Services;
using Xunit;

namespace TransactionService.Tests.UnitTests.Domain.Services
{
    public class TransactionDomainServiceTests
    {
        private readonly Mock<ITransactionRepository> _mockRepository;
        private readonly Mock<IAntiFraudService> _mockAntiFraudService;
        private readonly TransactionDomainService _transactionDomainService;
        private readonly Guid _sourceAccountId = Guid.NewGuid();
        private readonly Guid _targetAccountId = Guid.NewGuid();

        public TransactionDomainServiceTests()
        {
            _mockRepository = new Mock<ITransactionRepository>();
            _mockAntiFraudService = new Mock<IAntiFraudService>();
            
            _transactionDomainService = new TransactionDomainService(
                _mockRepository.Object,
                _mockAntiFraudService.Object);
        }

        [Fact]
        public async Task CreateTransactionAsync_ShouldCreateAndSendForValidation()
        {
            // Arrange
            const decimal transactionAmount = 100.50m;
            const int transferType = 1;
            
            Transaction capturedTransaction = null;
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Transaction>()))
                .Callback<Transaction>(t => capturedTransaction = t)
                .Returns(Task.CompletedTask);
            
            _mockAntiFraudService.Setup(s => s.SendTransactionForValidationAsync(It.IsAny<Transaction>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var result = await _transactionDomainService.CreateTransactionAsync(
                _sourceAccountId,
                _targetAccountId,
                transferType,
                transactionAmount);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(_sourceAccountId, result.SourceAccountId);
            Assert.Equal(_targetAccountId, result.TargetAccountId);
            Assert.Equal(transferType, result.TransferTypeId);
            Assert.Equal(transactionAmount, result.Value);
            Assert.Equal(TransactionStatus.Pending, result.Status);
            
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Once);
            
            _mockAntiFraudService.Verify(s => 
                s.SendTransactionForValidationAsync(It.IsAny<Transaction>()), 
                Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task CreateTransactionAsync_WithInvalidAmount_ShouldThrowException(decimal invalidAmount)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransactionDomainException>(() => 
                _transactionDomainService.CreateTransactionAsync(
                    _sourceAccountId,
                    _targetAccountId,
                    1,
                    invalidAmount));
            
            Assert.Contains("greater than zero", exception.Message);
            
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Never);
            _mockAntiFraudService.Verify(s => 
                s.SendTransactionForValidationAsync(It.IsAny<Transaction>()), 
                Times.Never);
        }

        [Fact]
        public async Task GetTransactionByExternalIdAsync_ShouldReturnTransaction()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var expectedTransaction = new Transaction(_sourceAccountId, _targetAccountId, 1, 500);
            
            _mockRepository.Setup(r => r.GetByExternalIdAsync(transactionId))
                .ReturnsAsync(expectedTransaction);
            
            // Act
            var result = await _transactionDomainService.GetTransactionByExternalIdAsync(transactionId);
            
            // Assert
            Assert.Same(expectedTransaction, result);
            _mockRepository.Verify(r => r.GetByExternalIdAsync(transactionId), Times.Once);
        }

        [Fact]
        public async Task GetTransactionByExternalIdAsync_WhenNotFound_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            
            _mockRepository.Setup(r => r.GetByExternalIdAsync(nonExistentId))
                .ReturnsAsync((Transaction)null);
            
            // Act
            var result = await _transactionDomainService.GetTransactionByExternalIdAsync(nonExistentId);
            
            // Assert
            Assert.Null(result);
            _mockRepository.Verify(r => r.GetByExternalIdAsync(nonExistentId), Times.Once);
        }

        [Fact]
        public async Task CreateTransactionAsync_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Transaction>()))
                .ThrowsAsync(new InvalidOperationException("Error de base de datos simulado"));
            
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _transactionDomainService.CreateTransactionAsync(
                    _sourceAccountId,
                    _targetAccountId,
                    1,
                    100));
        }
    }
} 