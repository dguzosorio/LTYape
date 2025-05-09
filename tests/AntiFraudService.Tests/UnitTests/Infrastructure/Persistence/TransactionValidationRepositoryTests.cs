using System;
using System.Threading.Tasks;
using AntiFraudService.Domain.Entities;
using AntiFraudService.Domain.Enums;
using AntiFraudService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AntiFraudService.Tests.UnitTests.Infrastructure.Persistence
{
    public class TransactionValidationRepositoryTests
    {
        private readonly DbContextOptions<AntiFraudDbContext> _contextOptions;
        private readonly Mock<ILogger<TransactionValidationRepository>> _mockLogger;
        
        public TransactionValidationRepositoryTests()
        {
            // Configurar opciones para usar base de datos en memoria
            _contextOptions = new DbContextOptionsBuilder<AntiFraudDbContext>()
                .UseInMemoryDatabase(databaseName: $"AntiFraudDb_{Guid.NewGuid()}")
                .Options;
            
            _mockLogger = new Mock<ILogger<TransactionValidationRepository>>();
            
            // Inicializar la base de datos con datos de prueba
            using var context = new AntiFraudDbContext(_contextOptions);
            context.Database.EnsureCreated();
            SeedDatabase(context);
        }
        
        private void SeedDatabase(AntiFraudDbContext context)
        {
            var accountId = Guid.Parse("1d86fa31-228f-4076-b82f-9a8e9adb8142");
            var today = DateTime.UtcNow.Date;
            
            // Agregar algunas validaciones para probar el cálculo del total diario
            context.TransactionValidations.AddRange(
                new TransactionValidation(
                    Guid.NewGuid(), 
                    accountId, 
                    1000, 
                    ValidationResult.Approved),
                new TransactionValidation(
                    Guid.NewGuid(), 
                    accountId, 
                    2000, 
                    ValidationResult.Approved),
                new TransactionValidation(
                    Guid.NewGuid(), 
                    accountId, 
                    3000, 
                    ValidationResult.Rejected, 
                    RejectionReason.ExceedsMaximumAmount),
                new TransactionValidation(
                    Guid.NewGuid(), 
                    Guid.NewGuid(), // Cuenta diferente
                    5000, 
                    ValidationResult.Approved)
            );
            
            context.SaveChanges();
        }
        
        [Fact]
        public async Task GetByTransactionExternalIdAsync_WhenExists_ShouldReturnValidation()
        {
            // Arrange
            using var context = new AntiFraudDbContext(_contextOptions);
            var repository = new TransactionValidationRepository(context, _mockLogger.Object);
            
            var transactionId = Guid.NewGuid();
            var validation = new TransactionValidation(
                transactionId,
                Guid.NewGuid(),
                1500,
                ValidationResult.Approved);
            
            await context.TransactionValidations.AddAsync(validation);
            await context.SaveChangesAsync();
            
            // Act
            var result = await repository.GetByTransactionExternalIdAsync(transactionId);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(transactionId, result.TransactionExternalId);
            Assert.Equal(1500, result.TransactionAmount);
            Assert.Equal(ValidationResult.Approved, result.Result);
        }
        
        [Fact]
        public async Task GetByTransactionExternalIdAsync_WhenDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            using var context = new AntiFraudDbContext(_contextOptions);
            var repository = new TransactionValidationRepository(context, _mockLogger.Object);
            var nonExistentId = Guid.NewGuid();
            
            // Act
            var result = await repository.GetByTransactionExternalIdAsync(nonExistentId);
            
            // Assert
            Assert.Null(result);
        }
        
        [Fact]
        public async Task AddAsync_ShouldAddNewValidation()
        {
            // Arrange
            using var context = new AntiFraudDbContext(_contextOptions);
            var repository = new TransactionValidationRepository(context, _mockLogger.Object);
            
            var transactionId = Guid.NewGuid();
            var validation = new TransactionValidation(
                transactionId,
                Guid.NewGuid(),
                2500,
                ValidationResult.Rejected,
                RejectionReason.ExceedsMaximumAmount,
                "Excede monto máximo");
            
            // Act
            await repository.AddAsync(validation);
            
            // Assert
            var savedValidation = await context.TransactionValidations
                .FirstOrDefaultAsync(v => v.TransactionExternalId == transactionId);
            
            Assert.NotNull(savedValidation);
            Assert.Equal(transactionId, savedValidation.TransactionExternalId);
            Assert.Equal(2500, savedValidation.TransactionAmount);
            Assert.Equal(ValidationResult.Rejected, savedValidation.Result);
            Assert.Equal(RejectionReason.ExceedsMaximumAmount, savedValidation.RejectionReason);
        }
        
        [Fact]
        public async Task AddAsync_WhenDuplicateTransaction_ShouldNotAddAgain()
        {
            // Arrange
            using var context = new AntiFraudDbContext(_contextOptions);
            var repository = new TransactionValidationRepository(context, _mockLogger.Object);
            
            var transactionId = Guid.NewGuid();
            
            // Primera validación
            var validation1 = new TransactionValidation(
                transactionId,
                Guid.NewGuid(),
                1000,
                ValidationResult.Approved);
            
            await repository.AddAsync(validation1);
            
            // Segunda validación con el mismo ID
            var validation2 = new TransactionValidation(
                transactionId,
                Guid.NewGuid(),
                2000, // Diferente monto
                ValidationResult.Rejected, // Diferente resultado
                RejectionReason.ExceedsMaximumAmount);
            
            // Act
            await repository.AddAsync(validation2);
            
            // Assert
            var count = await context.TransactionValidations
                .CountAsync(v => v.TransactionExternalId == transactionId);
            
            Assert.Equal(1, count); // Solo debería haber una validación
            
            var savedValidation = await context.TransactionValidations
                .FirstOrDefaultAsync(v => v.TransactionExternalId == transactionId);
            
            // Debería mantener los valores de la primera validación
            Assert.Equal(1000, savedValidation.TransactionAmount);
            Assert.Equal(ValidationResult.Approved, savedValidation.Result);
        }
        
        [Fact]
        public async Task GetDailyTransactionAmountForAccountAsync_ShouldReturnCorrectAmount()
        {
            // Arrange
            using var context = new AntiFraudDbContext(_contextOptions);
            var repository = new TransactionValidationRepository(context, _mockLogger.Object);
            
            var accountId = Guid.Parse("1d86fa31-228f-4076-b82f-9a8e9adb8142");
            var today = DateTime.UtcNow.Date;
            
            // Act
            var result = await repository.GetDailyTransactionAmountForAccountAsync(accountId, today);
            
            // Assert
            // Solo debe sumar las transacciones aprobadas (1000 + 2000 = 3000)
            // La rechazada de 3000 y la de otra cuenta de 5000 no se deben incluir
            Assert.Equal(3000, result);
        }
        
        [Fact]
        public async Task GetDailyTransactionAmountForAccountAsync_WithNoTransactions_ShouldReturnZero()
        {
            // Arrange
            using var context = new AntiFraudDbContext(_contextOptions);
            var repository = new TransactionValidationRepository(context, _mockLogger.Object);
            
            var nonExistentAccountId = Guid.NewGuid();
            var today = DateTime.UtcNow.Date;
            
            // Act
            var result = await repository.GetDailyTransactionAmountForAccountAsync(nonExistentAccountId, today);
            
            // Assert
            Assert.Equal(0, result);
        }
    }
} 