using System;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TransactionService.Infrastructure.Kafka
{
    /// <summary>
    /// Implementation of Kafka producer using Confluent.Kafka
    /// </summary>
    public class KafkaProducer : IKafkaProducer, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducer> _logger;
        
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
                ClientId = $"transaction-service-producer-{Guid.NewGuid()}"
            };
            
            _producer = new ProducerBuilder<string, string>(config).Build();
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
            try
            {
                var keyString = key?.ToString() ?? Guid.NewGuid().ToString();
                var valueJson = JsonSerializer.Serialize(value);
                
                var message = new Message<string, string>
                {
                    Key = keyString,
                    Value = valueJson
                };
                
                var deliveryResult = await _producer.ProduceAsync(topic, message);
                
                _logger.LogInformation(
                    "Produced message to {Topic} - Partition: {Partition}, Offset: {Offset}",
                    deliveryResult.Topic, deliveryResult.Partition, deliveryResult.Offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error producing message to {Topic}", topic);
                throw;
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