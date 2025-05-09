using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Services;
using TransactionService.Infrastructure.Kafka;
using TransactionService.Infrastructure.Kafka.Messages;

namespace TransactionService.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the anti-fraud service using Kafka
    /// </summary>
    public class KafkaAntiFraudService : IAntiFraudService
    {
        private readonly IKafkaProducer _kafkaProducer;
        private readonly IConfiguration _configuration;
        private readonly ILogger<KafkaAntiFraudService> _logger;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaAntiFraudService"/> class
        /// </summary>
        /// <param name="kafkaProducer">Kafka producer</param>
        /// <param name="configuration">Application configuration</param>
        /// <param name="logger">Logger</param>
        public KafkaAntiFraudService(
            IKafkaProducer kafkaProducer,
            IConfiguration configuration,
            ILogger<KafkaAntiFraudService> logger)
        {
            _kafkaProducer = kafkaProducer;
            _configuration = configuration;
            _logger = logger;
        }
        
        /// <summary>
        /// Sends a transaction to the anti-fraud service for validation
        /// </summary>
        /// <param name="transaction">The transaction to validate</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SendTransactionForValidationAsync(Transaction transaction)
        {
            var topic = _configuration["Kafka:Topics:TransactionValidationRequest"];
            if (string.IsNullOrEmpty(topic))
                throw new InvalidOperationException("Transaction validation request topic is not configured");

            var message = new TransactionValidationRequestMessage
            {
                TransactionExternalId = transaction.TransactionExternalId,
                SourceAccountId = transaction.SourceAccountId,
                TargetAccountId = transaction.TargetAccountId,
                TransferTypeId = transaction.TransferTypeId,
                Value = transaction.Value,
                CreatedAt = transaction.CreatedAt
            };

            await _kafkaProducer.ProduceAsync(topic, transaction.TransactionExternalId.ToString(), message);
        }
        
        private TransactionValidationRequestMessage MapTransactionToMessage(Transaction transaction)
        {
            return new TransactionValidationRequestMessage
            {
                TransactionExternalId = transaction.TransactionExternalId,
                SourceAccountId = transaction.SourceAccountId,
                TargetAccountId = transaction.TargetAccountId,
                TransferTypeId = transaction.TransferTypeId,
                Value = transaction.Value,
                CreatedAt = transaction.CreatedAt
            };
        }
    }
} 