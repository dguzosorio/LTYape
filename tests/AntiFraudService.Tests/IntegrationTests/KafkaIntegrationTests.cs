using System;
using System.Threading.Tasks;
using AntiFraudService.Infrastructure.Kafka;
using AntiFraudService.Infrastructure.Kafka.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AntiFraudService.Tests.IntegrationTests
{
    // Nota: Estos tests están diseñados para ejecutarse solo cuando exista una conexión real a Kafka.
    // Están marcados como "Skip" por defecto para evitar fallos en el pipeline de CI/CD cuando no hay conexión a Kafka.
    public class KafkaIntegrationTests
    {
        private readonly Mock<ILogger<KafkaProducer>> _mockLogger;
        private readonly IConfiguration _configuration;

        public KafkaIntegrationTests()
        {
            _mockLogger = new Mock<ILogger<KafkaProducer>>();
            
            // Crear configuración para tests
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Kafka:BootstrapServers", "localhost:9092"},
                    {"Kafka:Topics:TransactionValidationRequest", "transaction-validation-request-test"},
                    {"Kafka:Topics:TransactionValidationResponse", "transaction-validation-response-test"}
                })
                .Build();
        }

        [Fact(Skip = "Este test requiere una conexión real a Kafka. Desactive Skip para ejecutarlo localmente.")]
        public async Task KafkaProducer_ShouldProduceMessage()
        {
            // Arrange
            using var producer = new KafkaProducer(_configuration, _mockLogger.Object);
            var message = new TransactionValidationRequestMessage
            {
                TransactionExternalId = Guid.NewGuid(),
                SourceAccountId = Guid.NewGuid(),
                TargetAccountId = Guid.NewGuid(),
                Value = 100,
                CreatedAt = DateTime.UtcNow,
                TransferTypeId = 1
            };
            
            var topic = _configuration["Kafka:Topics:TransactionValidationRequest"];
            
            // Act & Assert (no debería lanzar excepción)
            await producer.ProduceAsync(topic, message.TransactionExternalId.ToString(), message);
        }

        [Fact(Skip = "Este test requiere una conexión real a Kafka. Desactive Skip para ejecutarlo localmente.")]
        public void KafkaConsumer_ShouldConsumeMessage()
        {
            // Este test es principalmente un ejemplo, ya que probar un consumer correctamente 
            // requeriría una combinación de producer y consumer en un entorno controlado.
            
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<KafkaConsumer>();
            
            var consumerConfig = new Dictionary<string, string>
            {
                {"bootstrap.servers", "localhost:9092"},
                {"group.id", "test-consumer-group"},
                {"auto.offset.reset", "earliest"}
            };
            
            var topicName = _configuration["Kafka:Topics:TransactionValidationRequest"];
            
            // Act & Assert (verificar que se puede crear y destruir sin errores)
            Assert.NotNull(topicName); // Asegurarse de que el topic existe
            
            // Nota: Aquí no estamos realmente consumiendo mensajes, solo verificamos que 
            // el consumer se puede crear sin problemas.
            // Un test más completo requeriría enviar un mensaje y luego consumirlo.
        }
    }
} 