using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AntiFraudService.Domain.Enums;
using AntiFraudService.Application.Services;
using AntiFraudService.Infrastructure.Kafka.Messages;

namespace AntiFraudService.Infrastructure.Kafka
{
    /// <summary>
    /// Hosted service for consuming Kafka messages
    /// </summary>
    public class KafkaConsumerHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IKafkaConsumer _kafkaConsumer;
        private readonly IConfiguration _configuration;
        private readonly ILogger<KafkaConsumerHostedService> _logger;
        private IDisposable? _subscription;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaConsumerHostedService"/> class
        /// </summary>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="kafkaConsumer">Kafka consumer</param>
        /// <param name="configuration">Application configuration</param>
        /// <param name="logger">Logger</param>
        public KafkaConsumerHostedService(
            IServiceProvider serviceProvider,
            IKafkaConsumer kafkaConsumer,
            IConfiguration configuration,
            ILogger<KafkaConsumerHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _kafkaConsumer = kafkaConsumer;
            _configuration = configuration;
            _logger = logger;
        }
        
        /// <summary>
        /// Executes the background service
        /// </summary>
        /// <param name="stoppingToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var requestTopic = _configuration["Kafka:Topics:TransactionValidationRequest"];
            
            if (string.IsNullOrEmpty(requestTopic))
            {
                _logger.LogError("The 'TransactionValidationRequest' topic configuration is not defined. The service will not start the subscription.");
                return Task.CompletedTask;
            }
            
            _logger.LogInformation("Starting Kafka consumer for topic: {Topic}", requestTopic);
            
            try 
            {
                _subscription = _kafkaConsumer.Subscribe<string, TransactionValidationRequestMessage>(
                    requestTopic,
                    (key, message) => HandleValidationRequestAsync(key, message).GetAwaiter().GetResult());
                
                _logger.LogInformation("Kafka subscription successfully started for topic: {Topic}", requestTopic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Kafka subscription for topic: {Topic}. Error: {ErrorMessage}", 
                    requestTopic, ex.Message);
            }
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Handles the validation request message
        /// </summary>
        /// <param name="key">Message key</param>
        /// <param name="message">Validation request message</param>
        private async Task HandleValidationRequestAsync(string key, TransactionValidationRequestMessage message)
        {
            if (message == null)
            {
                _logger.LogWarning("Received validation message is null");
                return;
            }
            
            // Validación usando un único método y retornando temprano en caso de error
            if (!IsValidTransactionMessage(message, out string validationError))
            {
                _logger.LogWarning("Invalid transaction data received: {Reason}, TransactionId: {TransactionId}", 
                    validationError, message.TransactionExternalId);
                return;
            }
            
            _logger.LogInformation("Validation request received for transaction: {TransactionId}, Amount: {Amount}, From: {SourceAccount} To: {TargetAccount}", 
                message.TransactionExternalId, message.Value, message.SourceAccountId, message.TargetAccountId);
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var antiFraudApplicationService = scope.ServiceProvider.GetRequiredService<IAntiFraudApplicationService>();
                
                if (antiFraudApplicationService == null)
                {
                    _logger.LogError("Could not resolve IAntiFraudApplicationService service");
                    return;
                }
                
                var validation = await antiFraudApplicationService.ValidateTransactionAsync(
                    message.TransactionExternalId,
                    message.SourceAccountId,
                    message.Value,
                    message.CreatedAt);
                
                if (validation == null)
                {
                    _logger.LogError("Validation returned null for transaction {TransactionId}", message.TransactionExternalId);
                    return;
                }
                
                _logger.LogInformation(
                    "Transaction {TransactionId} validated with result: {Result}, Reason: {Reason}, Notes: {Notes}",
                    message.TransactionExternalId, 
                    validation.Result,
                    validation.Result == ValidationResult.Rejected ? validation.RejectionReason.ToString() : "N/A",
                    validation.Notes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing validation request for transaction: {TransactionId}. Details: {ErrorMessage}", 
                    message.TransactionExternalId, ex.ToString());
                
                // Consider whether to retry or notify a monitoring system
            }
        }

        /// <summary>
        /// Verifica si un mensaje de transacción es válido
        /// </summary>
        /// <param name="message">El mensaje a validar</param>
        /// <param name="validationError">Mensaje de error de validación si falla</param>
        /// <returns>True si el mensaje es válido, False en caso contrario</returns>
        private bool IsValidTransactionMessage(TransactionValidationRequestMessage message, out string validationError)
        {
            // Uso pattern matching para validación más elegante
            validationError = message switch
            {
                { TransactionExternalId: Guid guid } when guid == Guid.Empty 
                    => "Transaction external ID is empty",
                    
                { SourceAccountId: Guid guid } when guid == Guid.Empty 
                    => "Source account ID is empty",
                    
                { TargetAccountId: Guid guid } when guid == Guid.Empty 
                    => "Target account ID is empty",
                    
                { Value: decimal value } when value <= 0 
                    => "Transaction value must be greater than zero",
                    
                { CreatedAt: DateTime date } when date == default || date > DateTime.UtcNow 
                    => "Creation date is invalid or in the future",
                    
                _ => string.Empty // Sin error
            };
            
            return string.IsNullOrEmpty(validationError);
        }
        
        /// <summary>
        /// Called when the service is stopping
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Kafka consumer");
            
            _subscription?.Dispose();
            
            return base.StopAsync(cancellationToken);
        }
    }
} 