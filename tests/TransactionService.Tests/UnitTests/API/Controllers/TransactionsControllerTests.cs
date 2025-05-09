using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TransactionService.API.Controllers;
using TransactionService.Application.DTOs;
using TransactionService.Application.Services;
using Xunit;

namespace TransactionService.Tests.UnitTests.API.Controllers
{
    public class TransactionsControllerTests
    {
        private readonly Mock<ITransactionApplicationService> _mockTransactionService;
        private readonly TransactionsController _controller;

        public TransactionsControllerTests()
        {
            _mockTransactionService = new Mock<ITransactionApplicationService>();
            _controller = new TransactionsController(_mockTransactionService.Object);
        }

        [Fact]
        public async Task GetTransaction_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = new GetTransactionRequest
            {
                TransactionExternalId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            var expectedResponse = new TransactionResponse
            {
                TransactionExternalId = request.TransactionExternalId,
                SourceAccountId = Guid.NewGuid(),
                TargetAccountId = Guid.NewGuid(),
                TransferTypeId = 1,
                Value = 100,
                Status = "Pending",
                CreatedAt = request.CreatedAt.Value,
                UpdatedAt = null
            };

            _mockTransactionService
                .Setup(x => x.GetTransactionAsync(It.IsAny<GetTransactionRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetTransaction(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<TransactionResponse>(okResult.Value);
            Assert.Equal(expectedResponse.TransactionExternalId, returnValue.TransactionExternalId);
            Assert.Equal(expectedResponse.CreatedAt, returnValue.CreatedAt);
        }

        [Fact]
        public async Task GetTransaction_WithNullRequest_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetTransaction(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetTransaction_WithEmptyExternalId_ReturnsBadRequest()
        {
            // Arrange
            var request = new GetTransactionRequest
            {
                TransactionExternalId = Guid.Empty,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _controller.GetTransaction(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetTransaction_WhenTransactionNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new GetTransactionRequest
            {
                TransactionExternalId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            _mockTransactionService
                .Setup(x => x.GetTransactionAsync(It.IsAny<GetTransactionRequest>()))
                .ReturnsAsync((TransactionResponse)null);

            // Act
            var result = await _controller.GetTransaction(request);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
} 