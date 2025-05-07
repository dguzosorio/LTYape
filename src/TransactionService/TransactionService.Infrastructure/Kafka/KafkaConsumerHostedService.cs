using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Domain.Repositories;
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
            var responseTopic = _configuration["Kafka:Topics:TransactionValidationResponse"];
            
            _logger.LogInformation("Starting Kafka consumer for topic: {Topic}", responseTopic);
            
            _subscription = _kafkaConsumer.Subscribe<string, TransactionValidationResponseMessage>(
                responseTopic,
                (key, message) => HandleValidationResponseAsync(key, message).GetAwaiter().GetResult());
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Handles the validation response message
        /// </summary>
        /// <param name="key">Message key</param>
        /// <param name="message">Validation response message</param>
        private async Task HandleValidationResponseAsync(string key, TransactionValidationResponseMessage message)
        {
            _logger.LogInformation("Received validation response for transaction: {TransactionId}", message.TransactionExternalId);
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var transactionRepository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                
                var transaction = await transactionRepository.GetByExternalIdAsync(message.TransactionExternalId);
                if (transaction == null)
                {
                    _logger.LogWarning("Transaction not found: {TransactionId}", message.TransactionExternalId);
                    return;
                }
                
                if (message.IsValid)
                {
                    transaction.Complete();
                }
                else
                {
                    transaction.Reject($"Rejected: {message.RejectionReason}. {message.Notes}");
                }
                
                await transactionRepository.UpdateAsync(transaction);
                
                _logger.LogInformation(
                    "Updated transaction {TransactionId} status to {Status}",
                    transaction.TransactionExternalId, transaction.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing validation response for transaction: {TransactionId}", message.TransactionExternalId);
            }
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