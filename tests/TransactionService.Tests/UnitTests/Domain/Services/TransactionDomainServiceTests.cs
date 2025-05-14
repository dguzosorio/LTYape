using System;
using System.Threading.Tasks;
using Moq;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Domain.Exceptions;
using TransactionService.Domain.Ports;
using TransactionService.Domain.Services;
using Xunit;

namespace TransactionService.Tests.UnitTests.Domain.Services
{
    public class TransactionDomainServiceTests
    {
        private readonly Mock<ITransactionRepositoryPort> _mockTransactionRepository;
        private readonly Mock<IAntiFraudEventPort> mockAntiFraudEventPort;
        private readonly TransactionDomainService _service;
        private readonly Guid _sourceAccountId = Guid.NewGuid();
        private readonly Guid _targetAccountId = Guid.NewGuid();

        public TransactionDomainServiceTests()
        {
            _mockTransactionRepository = new Mock<ITransactionRepositoryPort>();
            mockAntiFraudEventPort = new Mock<IAntiFraudEventPort>();
            _service = new TransactionDomainService(_mockTransactionRepository.Object, mockAntiFraudEventPort.Object);
        }

        [Fact]
        public async Task CreateTransactionAsync_ShouldCreateAndSendForValidation()
        {
            // Arrange
            const decimal transactionAmount = 100.50m;
            const int transferType = 1;
            
            Transaction capturedTransaction = null;
            _mockTransactionRepository.Setup(r => r.addAsync(It.IsAny<Transaction>()))
                .Callback<Transaction>(t => capturedTransaction = t)
                .Returns(Task.CompletedTask);
            
            mockAntiFraudEventPort.Setup(s => s.sendTransactionForValidationAsync(It.IsAny<Transaction>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var result = await _service.CreateTransactionAsync(
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
            
            _mockTransactionRepository.Verify(r => r.addAsync(It.IsAny<Transaction>()), Times.Once);
            
            mockAntiFraudEventPort.Verify(s => 
                s.sendTransactionForValidationAsync(It.IsAny<Transaction>()), 
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
                _service.CreateTransactionAsync(
                    _sourceAccountId,
                    _targetAccountId,
                    1,
                    invalidAmount));
            
            Assert.Contains("greater than zero", exception.Message);
            
            _mockTransactionRepository.Verify(r => r.addAsync(It.IsAny<Transaction>()), Times.Never);
            mockAntiFraudEventPort.Verify(s => 
                s.sendTransactionForValidationAsync(It.IsAny<Transaction>()), 
                Times.Never);
        }

        [Fact]
        public async Task GetTransactionByExternalIdAsync_ShouldReturnTransaction()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var expectedTransaction = new Transaction(_sourceAccountId, _targetAccountId, 1, 500);
            
            _mockTransactionRepository.Setup(r => r.getByExternalIdAndDateAsync(transactionId, It.IsAny<DateTime>()))
                .ReturnsAsync(expectedTransaction);
            
            // Act
            var result = await _service.GetTransactionByExternalIdAsync(transactionId);
            
            // Assert
            Assert.Same(expectedTransaction, result);
            _mockTransactionRepository.Verify(r => r.getByExternalIdAndDateAsync(transactionId, It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task GetTransactionByExternalIdAsync_WhenNotFound_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            
            _mockTransactionRepository.Setup(r => r.getByExternalIdAndDateAsync(nonExistentId, It.IsAny<DateTime>()))
                .ReturnsAsync((Transaction)null);
            
            // Act
            var result = await _service.GetTransactionByExternalIdAsync(nonExistentId);
            
            // Assert
            Assert.Null(result);
            _mockTransactionRepository.Verify(r => r.getByExternalIdAndDateAsync(nonExistentId, It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task CreateTransactionAsync_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            _mockTransactionRepository.Setup(r => r.addAsync(It.IsAny<Transaction>()))
                .ThrowsAsync(new InvalidOperationException("Error de base de datos simulado"));
            
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CreateTransactionAsync(
                    _sourceAccountId,
                    _targetAccountId,
                    1,
                    100));
        }

        [Fact]
        public async Task GetTransactionByExternalIdAndDateAsync_WhenTransactionExists_ReturnsTransaction()
        {
            // Arrange
            var externalId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;
            var expectedTransaction = new Transaction(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                100
            );

            _mockTransactionRepository
                .Setup(x => x.getByExternalIdAndDateAsync(externalId, createdAt))
                .ReturnsAsync(expectedTransaction);

            // Act
            var result = await _service.GetTransactionByExternalIdAndDateAsync(externalId, createdAt);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTransaction.TransactionExternalId, result.TransactionExternalId);
            Assert.Equal(expectedTransaction.CreatedAt, result.CreatedAt);
        }

        [Fact]
        public async Task GetTransactionByExternalIdAndDateAsync_WhenTransactionDoesNotExist_ReturnsNull()
        {
            // Arrange
            var externalId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;

            _mockTransactionRepository
                .Setup(x => x.getByExternalIdAndDateAsync(externalId, createdAt))
                .ReturnsAsync((Transaction)null);

            // Act
            var result = await _service.GetTransactionByExternalIdAndDateAsync(externalId, createdAt);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateTransactionAsync_WithValidData_CreatesAndSendsForValidation()
        {
            // Arrange
            var sourceAccountId = Guid.NewGuid();
            var targetAccountId = Guid.NewGuid();
            var transferTypeId = 1;
            var value = 100m;

            // Act
            var result = await _service.CreateTransactionAsync(sourceAccountId, targetAccountId, transferTypeId, value);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(sourceAccountId, result.SourceAccountId);
            Assert.Equal(targetAccountId, result.TargetAccountId);
            Assert.Equal(transferTypeId, result.TransferTypeId);
            Assert.Equal(value, result.Value);
            Assert.Equal(TransactionStatus.Pending, result.Status);

            _mockTransactionRepository.Verify(
                x => x.addAsync(It.IsAny<Transaction>()),
                Times.Once);

            mockAntiFraudEventPort.Verify(
                x => x.sendTransactionForValidationAsync(It.IsAny<Transaction>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateTransactionAsync_WithInvalidValue_ThrowsException()
        {
            // Arrange
            var sourceAccountId = Guid.NewGuid();
            var targetAccountId = Guid.NewGuid();
            var transferTypeId = 1;
            var value = 0m;

            // Act & Assert
            await Assert.ThrowsAsync<TransactionDomainException>(() =>
                _service.CreateTransactionAsync(sourceAccountId, targetAccountId, transferTypeId, value));
        }

        [Fact]
        public async Task UpdateTransactionStatusAsync_WithPendingTransaction_UpdatesStatus()
        {
            // Arrange
            var transaction = new Transaction(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                100
            );

            // Act
            await _service.UpdateTransactionStatusAsync(transaction, TransactionStatus.Approved);

            // Assert
            Assert.Equal(TransactionStatus.Approved, transaction.Status);
            _mockTransactionRepository.Verify(
                x => x.updateAsync(It.IsAny<Transaction>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionStatusAsync_WithNonPendingTransaction_ThrowsException()
        {
            // Arrange
            var transaction = new Transaction(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                100
            );
            transaction.Approve();

            // Act & Assert
            await Assert.ThrowsAsync<TransactionDomainException>(() =>
                _service.UpdateTransactionStatusAsync(transaction, TransactionStatus.Rejected));
        }
    }
} 