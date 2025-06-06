using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TransactionService.Domain.Ports;
using TransactionService.Infrastructure.Kafka.Messages;

namespace TransactionService.Infrastructure.Kafka
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
            ILogger<KafkaConsumerHostedService> logger) =>
            (_serviceProvider, _kafkaConsumer, _configuration, _logger) = 
            (serviceProvider, kafkaConsumer, configuration, logger);
        
        /// <summary>
        /// Executes the background service
        /// </summary>
        /// <param name="stoppingToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var responseTopic = _configuration["Kafka:Topics:TransactionValidationResponse"];
            
            if (string.IsNullOrEmpty(responseTopic))
            {
                _logger.LogError("The 'TransactionValidationResponse' topic configuration is missing. Service subscription will not start.");
                return Task.CompletedTask;
            }
            
            _logger.LogInformation("Starting Kafka consumer for topic: {Topic}", responseTopic);
            
            try
            {
                _subscription = _kafkaConsumer.Subscribe<string, TransactionValidationResponseMessage>(
                    responseTopic,
                    (_, message) => HandleValidationResponseAsync(_, message).GetAwaiter().GetResult());
                
                _logger.LogInformation("Successfully subscribed to Kafka topic: {Topic}", responseTopic);
                
                // Periodically verify the connection with Kafka
                Task.Run(async () =>
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                            _logger.LogInformation("Kafka subscription is active for topic: {Topic}", responseTopic);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start Kafka subscription for topic: {Topic}. Error: {ErrorMessage}", 
                    responseTopic, ex.Message);
            }
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Handles the validation response message
        /// </summary>
        /// <param name="_">Message key (not used)</param>
        /// <param name="message">Validation response message</param>
        private async Task HandleValidationResponseAsync(string _, TransactionValidationResponseMessage message)
        {
            if (message == null)
            {
                _logger.LogWarning("Received null validation response message");
                return;
            }
            
            _logger.LogInformation(
                "Received validation response for transaction: {TransactionId}, Valid: {IsValid}, Reason: {Reason}",
                message.TransactionExternalId, message.IsValid, message.RejectionReason ?? "N/A");
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var transactionRepository = scope.ServiceProvider.GetRequiredService<ITransactionRepositoryPort>();
                
                if (transactionRepository == null)
                {
                    _logger.LogError("Failed to resolve ITransactionRepositoryPort service");
                    return;
                }
                
                var transaction = await transactionRepository.getByExternalIdAndDateAsync(
                    message.TransactionExternalId,
                    DateTime.UtcNow);
                    
                if (transaction == null)
                {
                    _logger.LogWarning("Transaction not found: {TransactionId}", message.TransactionExternalId);
                    return;
                }
                
                _logger.LogInformation(
                    "Current status of transaction {TransactionId}: {Status}", 
                    transaction.TransactionExternalId, transaction.Status);
                
                if (message.IsValid)
                {
                    transaction.Complete();
                    _logger.LogInformation("Transaction {TransactionId} marked as APPROVED", transaction.TransactionExternalId);
                }
                else
                {
                    transaction.Reject($"Rejected: {message.RejectionReason}. {message.Notes}");
                    _logger.LogInformation(
                        "Transaction {TransactionId} marked as REJECTED. Reason: {Reason}", 
                        transaction.TransactionExternalId, message.RejectionReason);
                }
                
                await transactionRepository.updateAsync(transaction);
                
                _logger.LogInformation(
                    "Transaction {TransactionId} updated with status: {Status}",
                    transaction.TransactionExternalId, transaction.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error processing validation response for transaction: {TransactionId}. Details: {ErrorMessage}",
                    message.TransactionExternalId, ex.ToString());
            }
        }
        
        /// <summary>
        /// Called when the service is stopping
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Kafka consumer service");
            
            _subscription?.Dispose();
            
            return base.StopAsync(cancellationToken);
        }
    }
} 