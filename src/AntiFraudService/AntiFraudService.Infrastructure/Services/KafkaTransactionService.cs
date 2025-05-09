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
using System.Threading;

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
        private const int MaxRetries = 3;
        
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
            var topic = _configuration["Kafka:Topics:TransactionValidationResponse"];
            if (string.IsNullOrEmpty(topic))
            {
                _logger.LogError("The Kafka topic configuration 'TransactionValidationResponse' is not defined");
                throw new InvalidOperationException("Incomplete or invalid Kafka configuration");
            }
            
            _logger.LogInformation(
                "Attempting to send validation response for transaction {TransactionId}. Topic: {Topic}",
                response.TransactionExternalId, topic);
            
            var message = MapResponseToMessage(response);
            var retryCount = 0;
            var success = false;
            
            while (!success && retryCount < MaxRetries)
            {
                try
                {
                    await _kafkaProducer.ProduceAsync(
                        topic,
                        response.TransactionExternalId.ToString(),
                        message);
                    
                    _logger.LogInformation(
                        "Validation response for transaction {TransactionId} successfully sent on attempt {RetryCount}",
                        response.TransactionExternalId, retryCount + 1);
                    
                    success = true;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    
                    if (retryCount >= MaxRetries)
                    {
                        _logger.LogError(
                            ex,
                            "Final error sending validation response for transaction {TransactionId} after {RetryCount} attempts: {ErrorMessage}",
                            response.TransactionExternalId, retryCount, ex.Message);
                        
                        throw;
                    }
                    
                    _logger.LogWarning(
                        "Error sending validation response for transaction {TransactionId} (attempt {RetryCount}/{MaxRetries}): {ErrorMessage}",
                        response.TransactionExternalId, retryCount, MaxRetries, ex.Message);
                    
                    // Wait before retrying (exponential backoff)
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));
                }
            }
        }
        
        /// <summary>
        /// Sends the validation result back to the transaction service
        /// </summary>
        /// <param name="validation">The transaction validation result</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SendValidationResultAsync(TransactionValidation validation)
        {
            var topic = _configuration["Kafka:Topics:TransactionValidationResponse"];
            if (string.IsNullOrEmpty(topic))
                throw new InvalidOperationException("Transaction validation response topic is not configured");

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