using System;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AntiFraudService.Infrastructure.Kafka
{
    /// <summary>
    /// Implementation of Kafka producer using Confluent.Kafka
    /// </summary>
    public class KafkaProducer : IKafkaProducer, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducer> _logger;
        private const int MaxRetries = 5;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaProducer"/> class
        /// </summary>
        /// <param name="configuration">Application configuration</param>
        /// <param name="logger">Logger</param>
        public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
        {
            _logger = logger;
            
            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                ClientId = $"antifraud-service-producer-{Guid.NewGuid()}",
                // Additional configuration to improve resilience
                MessageSendMaxRetries = 3,
                RetryBackoffMs = 1000,
                EnableIdempotence = true,
                Acks = Acks.All
            };
            
            _producer = new ProducerBuilder<string, string>(config).Build();
            
            _logger.LogInformation("Kafka producer initialized with server: {Server}", configuration["Kafka:BootstrapServers"]);
        }
        
        /// <summary>
        /// Produces a message to the specified Kafka topic
        /// </summary>
        /// <typeparam name="TKey">The type of the message key</typeparam>
        /// <typeparam name="TValue">The type of the message value</typeparam>
        /// <param name="topic">The Kafka topic</param>
        /// <param name="key">The message key</param>
        /// <param name="value">The message value</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task ProduceAsync<TKey, TValue>(string topic, TKey key, TValue value)
        {
            var retryCount = 0;
            var messageKey = key?.ToString() ?? Guid.NewGuid().ToString();
            var messageValue = JsonSerializer.Serialize(value);
            var message = new Message<string, string>
            {
                Key = messageKey,
                Value = messageValue
            };
            
            while (retryCount < MaxRetries)
            {
                try
                {
                    _logger.LogInformation(
                        "Attempting to produce message to {Topic}. Attempt: {RetryCount}/{MaxRetries}",
                        topic, retryCount + 1, MaxRetries);
                
                    var deliveryResult = await _producer.ProduceAsync(topic, message);
                    
                    _logger.LogInformation(
                        "Message successfully produced to {Topic} - Partition: {Partition}, Offset: {Offset}",
                        deliveryResult.Topic, deliveryResult.Partition, deliveryResult.Offset);
                    
                    return;
                }
                catch (ProduceException<string, string> ex)
                {
                    retryCount++;
                    
                    if (retryCount >= MaxRetries)
                    {
                        _logger.LogError(
                            ex, 
                            "Final error producing message to {Topic} after {RetryCount} attempts: {ErrorMessage}",
                            topic, retryCount, ex.Message);
                        
                        throw;
                    }
                    
                    _logger.LogWarning(
                        "Error producing message to {Topic} (attempt {RetryCount}/{MaxRetries}): {ErrorMessage}. Retrying...",
                        topic, retryCount, MaxRetries, ex.Message);
                    
                    // Wait before retrying (exponential backoff)
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error producing message to {Topic}: {ErrorMessage}", topic, ex.Message);
                    throw;
                }
            }
        }
        
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
} 