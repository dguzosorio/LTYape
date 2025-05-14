using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AntiFraudService.Domain.Models;
using AntiFraudService.Domain.Ports;
using AntiFraudService.Infrastructure.Kafka;
using AntiFraudService.Infrastructure.Kafka.Messages;

namespace AntiFraudService.Infrastructure.Services
{
    /// <summary>
    /// Implementación del puerto de consumidor de eventos de transacciones usando Kafka
    /// </summary>
    public class KafkaTransactionEventConsumerService : ITransactionEventConsumerPort
    {
        private readonly IKafkaConsumer _kafkaConsumer;
        private readonly IConfiguration _configuration;
        private readonly ILogger<KafkaTransactionEventConsumerService> _logger;
        private IDisposable? _subscription = null;
        
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="KafkaTransactionEventConsumerService"/>
        /// </summary>
        /// <param name="kafkaConsumer">Consumidor de Kafka</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <param name="logger">Logger</param>
        public KafkaTransactionEventConsumerService(
            IKafkaConsumer kafkaConsumer,
            IConfiguration configuration,
            ILogger<KafkaTransactionEventConsumerService> logger)
        {
            _kafkaConsumer = kafkaConsumer;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Suscribe una función de callback para procesar eventos de validación de transacciones
        /// </summary>
        /// <param name="handler">La función que procesará los eventos de transacciones</param>
        /// <returns>Una tarea que representa la operación asíncrona</returns>
        public async Task subscribeToTransactionValidationRequestsAsync(Func<TransactionValidationRequest, Task> handler)
        {
            var topic = _configuration["Kafka:Topics:TransactionValidationRequest"];
            if (string.IsNullOrEmpty(topic))
            {
                _logger.LogError("La configuración del tema de Kafka 'TransactionValidationRequest' no está definida");
                throw new InvalidOperationException("Configuración de Kafka incompleta o inválida");
            }
            
            _logger.LogInformation("Suscribiendo al tema de Kafka {Topic} para solicitudes de validación", topic);
            
            // Cancelar cualquier suscripción existente
            _subscription?.Dispose();
            
            // Suscribirse al tema para recibir mensajes
            _subscription = _kafkaConsumer.Subscribe<string, TransactionValidationRequestMessage>(
                topic, 
                async (key, message) => 
                {
                    try
                    {
                        var request = MapMessageToRequest(message);
                        await handler(request);
                        _logger.LogInformation(
                            "Mensaje de validación de transacción {TransactionId} procesado correctamente",
                            request.TransactionExternalId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error al procesar mensaje de validación de transacción: {ErrorMessage}",
                            ex.Message);
                        // La suscripción manejará automáticamente los reintentos según la configuración de Kafka
                    }
                });
            
            await Task.CompletedTask;
        }
        
        private TransactionValidationRequest MapMessageToRequest(TransactionValidationRequestMessage message)
        {
            return new TransactionValidationRequest
            {
                TransactionExternalId = message.TransactionExternalId,
                SourceAccountId = message.SourceAccountId,
                TargetAccountId = message.TargetAccountId,
                TransferTypeId = message.TransferTypeId,
                Value = message.Value,
                CreatedAt = message.CreatedAt
            };
        }
    }
} 