using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AntiFraudService.Domain.Entities;
using AntiFraudService.Domain.Enums;
using AntiFraudService.Domain.Services;
using AntiFraudService.Infrastructure.Kafka;
using AntiFraudService.Infrastructure.Kafka.Messages;
using AntiFraudService.Domain.Models;

namespace AntiFraudService.Infrastructure.Services
{
    /// <summary>
    /// Implementation of transaction service using Kafka
    /// </summary>
    public class KafkaTransactionService : ITransactionService
    {
        private readonly IKafkaProducer _kafkaProducer;
        private readonly IConfiguration _configuration;
        private readonly ILogger<KafkaTransactionService> _logger;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaTransactionService"/> class
        /// </summary>
        /// <param name="kafkaProducer">Kafka producer</param>
        /// <param name="configuration">Application configuration</param>
        /// <param name="logger">Logger</param>
        public KafkaTransactionService(
            IKafkaProducer kafkaProducer,
            IConfiguration configuration,
            ILogger<KafkaTransactionService> logger)
        {
            _kafkaProducer = kafkaProducer;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Sends a validation response back to the Transaction service
        /// </summary>
        /// <param name="response">The validation response to send</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SendValidationResponseAsync(ValidationResponse response)
        {
            try
            {
                var topic = _configuration["Kafka:Topics:TransactionValidationResponse"];
                
                var message = MapResponseToMessage(response);
                
                _logger.LogInformation(
                    "Sending validation response for transaction {TransactionId}",
                    response.TransactionExternalId);
                
                await _kafkaProducer.ProduceAsync(
                    topic,
                    response.TransactionExternalId.ToString(),
                    message);
                
                _logger.LogInformation(
                    "Validation response for transaction {TransactionId} sent",
                    response.TransactionExternalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error sending validation response for transaction {TransactionId}",
                    response.TransactionExternalId);
                
                throw;
            }
        }
        
        /// <summary>
        /// Sends the validation result back to the transaction service
        /// </summary>
        /// <param name="validation">The transaction validation result</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SendValidationResultAsync(TransactionValidation validation)
        {
            try
            {
                var topic = _configuration["Kafka:Topics:TransactionValidationResponse"];
                
                var message = MapValidationToMessage(validation);
                
                _logger.LogInformation(
                    "Sending validation result for transaction {TransactionId}",
                    validation.TransactionExternalId);
                
                await _kafkaProducer.ProduceAsync(
                    topic,
                    validation.TransactionExternalId.ToString(),
                    message);
                
                _logger.LogInformation(
                    "Validation result for transaction {TransactionId} sent",
                    validation.TransactionExternalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error sending validation result for transaction {TransactionId}",
                    validation.TransactionExternalId);
                
                throw;
            }
        }
        
        private TransactionValidationResponseMessage MapValidationToMessage(TransactionValidation validation)
        {
            return new TransactionValidationResponseMessage
            {
                TransactionExternalId = validation.TransactionExternalId,
                IsValid = validation.Result == ValidationResult.Approved,
                RejectionReason = validation.Result == ValidationResult.Rejected 
                    ? validation.RejectionReason.ToString() 
                    : null,
                Notes = validation.Notes
            };
        }

        private TransactionValidationResponseMessage MapResponseToMessage(ValidationResponse response)
        {
            return new TransactionValidationResponseMessage
            {
                TransactionExternalId = response.TransactionExternalId,
                IsValid = response.Result == ValidationResult.Approved,
                RejectionReason = response.Result == ValidationResult.Rejected 
                    ? response.RejectionReason.ToString() 
                    : null,
                Notes = response.Notes
            };
        }
    }
} 