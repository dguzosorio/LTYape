using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AntiFraudService.Infrastructure.Kafka
{
    /// <summary>
    /// Implementation of Kafka consumer using Confluent.Kafka
    /// </summary>
    public class KafkaConsumer : IKafkaConsumer
    {
        private readonly Dictionary<string, IConsumer<string, string>> _consumers;
        private readonly Dictionary<string, CancellationTokenSource> _cancellationTokens;
        private readonly IConfiguration _configuration;
        private readonly ILogger<KafkaConsumer> _logger;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaConsumer"/> class
        /// </summary>
        /// <param name="configuration">Application configuration</param>
        /// <param name="logger">Logger</param>
        public KafkaConsumer(IConfiguration configuration, ILogger<KafkaConsumer> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _consumers = new Dictionary<string, IConsumer<string, string>>();
            _cancellationTokens = new Dictionary<string, CancellationTokenSource>();
        }
        
        /// <summary>
        /// Subscribes to a Kafka topic
        /// </summary>
        /// <typeparam name="TKey">The type of the message key</typeparam>
        /// <typeparam name="TValue">The type of the message value</typeparam>
        /// <param name="topic">The Kafka topic</param>
        /// <param name="handler">The message handler delegate</param>
        /// <returns>A disposable subscription</returns>
        public IDisposable Subscribe<TKey, TValue>(string topic, Action<TKey, TValue> handler)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                GroupId = _configuration["Kafka:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };
            
            var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(topic);
            
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            
            _consumers[topic] = consumer;
            _cancellationTokens[topic] = cancellationTokenSource;
            
            var subscriptionKey = $"{topic}_{Guid.NewGuid()}";
            
            new Thread(() => ConsumeMessages(consumer, handler, cancellationToken))
            {
                IsBackground = true
            }.Start();
            
            return new Subscription(() => Unsubscribe(topic));
        }
        
        private void ConsumeMessages<TKey, TValue>(
            IConsumer<string, string> consumer, 
            Action<TKey, TValue> handler,
            CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(cancellationToken);
                        if (consumeResult != null)
                        {
                            var key = typeof(TKey) == typeof(string) 
                                ? (TKey)(object)consumeResult.Message.Key 
                                : JsonSerializer.Deserialize<TKey>(consumeResult.Message.Key);
                            
                            var value = JsonSerializer.Deserialize<TValue>(
                                consumeResult.Message.Value,
                                new JsonSerializerOptions 
                                { 
                                    PropertyNameCaseInsensitive = true 
                                });
                            
                            if (key != null && value != null)
                            {
                                handler(key, value);
                                
                                _logger.LogInformation(
                                    "Consumed message from {Topic} - Partition: {Partition}, Offset: {Offset}",
                                    consumeResult.Topic, consumeResult.Partition, consumeResult.Offset);
                            }
                            else
                            {
                                _logger.LogWarning("Deserialized message key or value is null");
                            }
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming message");
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error while consuming messages: {Message}", ex.Message);
                    }
                }
            }
            finally
            {
                consumer.Close();
            }
        }
        
        private void Unsubscribe(string topic)
        {
            if (_cancellationTokens.TryGetValue(topic, out var tokenSource))
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
                _cancellationTokens.Remove(topic);
            }
            
            if (_consumers.TryGetValue(topic, out var consumer))
            {
                consumer.Close();
                consumer.Dispose();
                _consumers.Remove(topic);
            }
        }
        
        private class Subscription : IDisposable
        {
            private readonly Action _unsubscribeAction;
            
            public Subscription(Action unsubscribeAction)
            {
                _unsubscribeAction = unsubscribeAction;
            }
            
            public void Dispose()
            {
                _unsubscribeAction();
            }
        }
    }
} 