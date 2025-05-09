using System.Threading.Tasks;

namespace TransactionService.Infrastructure.Kafka
{
    /// <summary>
    /// Interface for Kafka producer
    /// </summary>
    public interface IKafkaProducer
    {
        /// <summary>
        /// Produces a message to the specified Kafka topic
        /// </summary>
        /// <typeparam name="TKey">The type of the message key</typeparam>
        /// <typeparam name="TValue">The type of the message value</typeparam>
        /// <param name="topic">The Kafka topic</param>
        /// <param name="key">The message key</param>
        /// <param name="value">The message value</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task ProduceAsync<TKey, TValue>(string topic, TKey key, TValue value);
    }
} 