using System;
using System.Threading.Tasks;
using Moq;
using TransactionService.Application.DTOs;
using TransactionService.Application.Services;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Services;
using Xunit;

namespace TransactionService.Tests.UnitTests.Application.Services
{
    public class TransactionApplicationServiceTests
    {
        private readonly Mock<ITransactionDomainService> _mockTransactionDomainService;
        private readonly TransactionApplicationService _service;

        public TransactionApplicationServiceTests()
        {
            _mockTransactionDomainService = new Mock<ITransactionDomainService>();
            _service = new TransactionApplicationService(_mockTransactionDomainService.Object);
        }

        [Fact]
        public async Task GetTransactionAsync_WithCreatedAt_ReturnsTransactionResponse()
        {
            // Arrange
            var request = new GetTransactionRequest
            {
                TransactionExternalId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            var domainTransaction = new Transaction(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                100
            );

            _mockTransactionDomainService
                .Setup(x => x.GetTransactionByExternalIdAndDateAsync(
                    request.TransactionExternalId,
                    request.CreatedAt.Value))
                .ReturnsAsync(domainTransaction);

            // Act
            var result = await _service.GetTransactionAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(domainTransaction.TransactionExternalId, result.TransactionExternalId);
            Assert.Equal(domainTransaction.CreatedAt, result.CreatedAt);
        }

        [Fact]
        public async Task GetTransactionAsync_WithoutCreatedAt_UsesCurrentDate()
        {
            // Arrange
            var request = new GetTransactionRequest
            {
                TransactionExternalId = Guid.NewGuid(),
                CreatedAt = null
            };

            var domainTransaction = new Transaction(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                100
            );

            _mockTransactionDomainService
                .Setup(x => x.GetTransactionByExternalIdAndDateAsync(
                    request.TransactionExternalId,
                    It.IsAny<DateTime>()))
                .ReturnsAsync(domainTransaction);

            // Act
            var result = await _service.GetTransactionAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(domainTransaction.TransactionExternalId, result.TransactionExternalId);
            Assert.Equal(domainTransaction.CreatedAt, result.CreatedAt);

            _mockTransactionDomainService.Verify(
                x => x.GetTransactionByExternalIdAndDateAsync(
                    request.TransactionExternalId,
                    It.IsAny<DateTime>()),
                Times.Once);
        }

        [Fact]
        public async Task GetTransactionAsync_WhenTransactionNotFound_ReturnsNull()
        {
            // Arrange
            var request = new GetTransactionRequest
            {
                TransactionExternalId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            _mockTransactionDomainService
                .Setup(x => x.GetTransactionByExternalIdAndDateAsync(
                    request.TransactionExternalId,
                    request.CreatedAt.Value))
                .ReturnsAsync((Transaction)null);

            // Act
            var result = await _service.GetTransactionAsync(request);

            // Assert
            Assert.Null(result);
        }
    }
} 