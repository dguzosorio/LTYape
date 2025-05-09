using System;

namespace TransactionService.Infrastructure.Kafka
{
    /// <summary>
    /// Interface for Kafka consumer
    /// </summary>
    public interface IKafkaConsumer
    {
        /// <summary>
        /// Subscribes to a Kafka topic
        /// </summary>
        /// <typeparam name="TKey">The type of the message key</typeparam>
        /// <typeparam name="TValue">The type of the message value</typeparam>
        /// <param name="topic">The Kafka topic</param>
        /// <param name="handler">The message handler delegate</param>
        /// <returns>A disposable subscription</returns>
        IDisposable Subscribe<TKey, TValue>(string topic, Action<TKey, TValue> handler);
    }
} 