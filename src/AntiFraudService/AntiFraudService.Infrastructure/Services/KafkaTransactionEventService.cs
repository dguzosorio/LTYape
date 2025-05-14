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
    /// Implementación del puerto de eventos de transacciones usando Kafka
    /// </summary>
    public class KafkaTransactionEventService : ITransactionEventPort
    {
        private readonly IKafkaProducer _kafkaProducer;
        private readonly IConfiguration _configuration;
        private readonly ILogger<KafkaTransactionEventService> _logger;
        private const int MaxRetries = 3;
        
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="KafkaTransactionEventService"/>
        /// </summary>
        /// <param name="kafkaProducer">Productor de Kafka</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <param name="logger">Logger</param>
        public KafkaTransactionEventService(
            IKafkaProducer kafkaProducer,
            IConfiguration configuration,
            ILogger<KafkaTransactionEventService> logger)
        {
            _kafkaProducer = kafkaProducer;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Envía una respuesta de validación al servicio de transacciones
        /// </summary>
        /// <param name="response">La respuesta de validación a enviar</param>
        /// <returns>Una tarea que representa la operación asíncrona</returns>
        public async Task sendValidationResponseAsync(ValidationResponse response)
        {
            var topic = _configuration["Kafka:Topics:TransactionValidationResponse"];
            if (string.IsNullOrEmpty(topic))
            {
                _logger.LogError("La configuración del tema de Kafka 'TransactionValidationResponse' no está definida");
                throw new InvalidOperationException("Configuración de Kafka incompleta o inválida");
            }
            
            _logger.LogInformation(
                "Intentando enviar respuesta de validación para la transacción {TransactionId}. Tema: {Topic}",
                response.TransactionExternalId, topic);
            
            var message = MapResponseToMessage(response);
            var retryCount = 0;
            var success = false;
            
            while (!success && retryCount < MaxRetries)
            {
                try
                {
                    await _kafkaProducer.ProduceAsync(
                        topic,
                        response.TransactionExternalId.ToString(),
                        message);
                    
                    _logger.LogInformation(
                        "Respuesta de validación para la transacción {TransactionId} enviada exitosamente en el intento {RetryCount}",
                        response.TransactionExternalId, retryCount + 1);
                    
                    success = true;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    
                    if (retryCount >= MaxRetries)
                    {
                        _logger.LogError(
                            ex,
                            "Error final al enviar respuesta de validación para la transacción {TransactionId} después de {RetryCount} intentos: {ErrorMessage}",
                            response.TransactionExternalId, retryCount, ex.Message);
                        
                        throw;
                    }
                    
                    _logger.LogWarning(
                        "Error al enviar respuesta de validación para la transacción {TransactionId} (intento {RetryCount}/{MaxRetries}): {ErrorMessage}",
                        response.TransactionExternalId, retryCount, MaxRetries, ex.Message);
                    
                    // Esperar antes de reintentar (retroceso exponencial)
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));
                }
            }
        }
        
        private TransactionValidationResponseMessage MapResponseToMessage(ValidationResponse response)
        {
            return new TransactionValidationResponseMessage
            {
                TransactionExternalId = response.TransactionExternalId,
                IsValid = response.Result == Domain.Enums.ValidationResult.Approved,
                RejectionReason = response.Result == Domain.Enums.ValidationResult.Rejected 
                    ? response.RejectionReason.ToString() 
                    : null,
                Notes = response.Notes
            };
        }
    }
} 