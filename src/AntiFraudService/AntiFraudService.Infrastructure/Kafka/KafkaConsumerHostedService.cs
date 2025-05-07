using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AntiFraudService.Domain.Entities;
using AntiFraudService.Domain.Enums;
using AntiFraudService.Domain.Repositories;
using AntiFraudService.Domain.Services;
using AntiFraudService.Infrastructure.Kafka.Messages;
using AntiFraudService.Application.Services;

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
        private IDisposable _subscription;
        
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
            
            _logger.LogInformation("Starting Kafka consumer for topic: {Topic}", requestTopic);
            
            _subscription = _kafkaConsumer.Subscribe<string, TransactionValidationRequestMessage>(
                requestTopic,
                (key, message) => HandleValidationRequestAsync(key, message).GetAwaiter().GetResult());
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Handles the validation request message
        /// </summary>
        /// <param name="key">Message key</param>
        /// <param name="message">Validation request message</param>
        private async Task HandleValidationRequestAsync(string key, TransactionValidationRequestMessage message)
        {
            _logger.LogInformation("Received validation request for transaction: {TransactionId}", message.TransactionExternalId);
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var antiFraudApplicationService = scope.ServiceProvider.GetRequiredService<IAntiFraudApplicationService>();
                
                var validation = await antiFraudApplicationService.ValidateTransactionAsync(
                    message.TransactionExternalId,
                    message.SourceAccountId,
                    message.Value,
                    message.CreatedAt);
                
                _logger.LogInformation(
                    "Validated transaction {TransactionId} with result: {Result}",
                    message.TransactionExternalId, validation.Result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing validation request for transaction: {TransactionId}", message.TransactionExternalId);
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