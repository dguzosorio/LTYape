using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Ports;
using TransactionService.Infrastructure.Kafka;
using TransactionService.Infrastructure.Kafka.Messages;
using TransactionService.Infrastructure.Services;
using Xunit;

namespace TransactionService.Tests.UnitTests.Infrastructure.Services
{
    public class KafkaAntiFraudServiceTests
    {
        private readonly Mock<IKafkaProducer> _mockKafkaProducer;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<KafkaAntiFraudService>> _mockLogger;
        private readonly KafkaAntiFraudService _service;
        private readonly string _topicName = "test-validation-request";

        public KafkaAntiFraudServiceTests()
        {
            _mockKafkaProducer = new Mock<IKafkaProducer>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<KafkaAntiFraudService>>();

            _mockConfiguration.Setup(c => c["Kafka:Topics:TransactionValidationRequest"])
                .Returns(_topicName);

            _service = new KafkaAntiFraudService(
                _mockKafkaProducer.Object,
                _mockConfiguration.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task SendTransactionForValidationAsync_ShouldSendMessageToKafka()
        {
            // Arrange
            var transaction = new Transaction(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                100
            );

            _mockKafkaProducer.Setup(p => p.ProduceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<TransactionValidationRequestMessage>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.sendTransactionForValidationAsync(transaction);

            // Assert
            _mockKafkaProducer.Verify(p => p.ProduceAsync(
                _topicName,
                transaction.TransactionExternalId.ToString(),
                It.Is<TransactionValidationRequestMessage>(msg =>
                    msg.TransactionExternalId == transaction.TransactionExternalId &&
                    msg.SourceAccountId == transaction.SourceAccountId &&
                    msg.TargetAccountId == transaction.TargetAccountId &&
                    msg.TransferTypeId == transaction.TransferTypeId &&
                    msg.Value == transaction.Value &&
                    msg.CreatedAt == transaction.CreatedAt)),
                Times.Once);
        }

        [Fact]
        public async Task SendTransactionForValidationAsync_WhenTopicNotConfigured_ShouldThrowException()
        {
            // Arrange
            var transaction = new Transaction(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                100
            );

            _mockConfiguration.Setup(c => c["Kafka:Topics:TransactionValidationRequest"])
                .Returns((string)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.sendTransactionForValidationAsync(transaction));

            Assert.Contains("topic is not configured", exception.Message);
            _mockKafkaProducer.Verify(p => p.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TransactionValidationRequestMessage>()),
                Times.Never);
        }
    }
} 