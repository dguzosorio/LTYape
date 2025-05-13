using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AntiFraudService.Domain.Entities;
using AntiFraudService.Domain.Ports;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace AntiFraudService.Infrastructure.Persistence
{
    /// <summary>
    /// Adaptador de repositorio de validación de transacciones utilizando Entity Framework Core
    /// </summary>
    public class TransactionValidationRepositoryAdapter : ITransactionValidationRepositoryPort
    {
        private readonly AntiFraudDbContext _dbContext;
        private readonly ILogger<TransactionValidationRepositoryAdapter> _logger;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TransactionValidationRepositoryAdapter"/>
        /// </summary>
        /// <param name="dbContext">El contexto de base de datos</param>
        /// <param name="logger">El logger</param>
        public TransactionValidationRepositoryAdapter(
            AntiFraudDbContext dbContext,
            ILogger<TransactionValidationRepositoryAdapter> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Recupera una validación de transacción por el identificador externo de la transacción
        /// </summary>
        /// <param name="transactionExternalId">ID externo de la transacción</param>
        /// <returns>La validación de transacción si se encuentra, o null si no se encuentra</returns>
        public async Task<TransactionValidation> getByTransactionExternalIdAsync(Guid transactionExternalId)
        {
            var validation = await _dbContext.TransactionValidations
                .FirstOrDefaultAsync(v => v.TransactionExternalId == transactionExternalId);
                
            if (validation == null)
            {
                throw new KeyNotFoundException($"No se encontró la validación para la transacción {transactionExternalId}");
            }
            
            return validation;
        }

        /// <summary>
        /// Añade una nueva validación de transacción al repositorio
        /// </summary>
        /// <param name="validation">La validación de transacción a añadir</param>
        public async Task addAsync(TransactionValidation validation)
        {
            try
            {
                // Verificar si la transacción ya existe para evitar duplicados
                var existingValidation = await _dbContext.TransactionValidations
                    .FirstOrDefaultAsync(v => v.TransactionExternalId == validation.TransactionExternalId);
                
                if (existingValidation != null)
                {
                    _logger.LogWarning(
                        "Se recibió un intento de validación duplicado para la transacción {TransactionId}. Esta validación será ignorada.",
                        validation.TransactionExternalId);
                    return; // No hacer nada si la validación ya existe
                }
                
                await _dbContext.TransactionValidations.AddAsync(validation);
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation(
                    "Validación de transacción {TransactionId} almacenada exitosamente con resultado {Result}",
                    validation.TransactionExternalId, validation.Result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al guardar la validación de transacción {TransactionId}: {ErrorMessage}",
                    validation.TransactionExternalId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Calcula el monto total de transacciones para una cuenta específica en una fecha específica
        /// </summary>
        /// <param name="sourceAccountId">El identificador de la cuenta</param>
        /// <param name="date">La fecha para la cual calcular el total</param>
        /// <returns>La suma de todos los montos de transacciones para la cuenta en la fecha especificada</returns>
        public async Task<decimal> getDailyTransactionAmountForAccountAsync(Guid sourceAccountId, DateTime date)
        {
            // Obtener inicio y fin del día
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            // Consultar transacciones aprobadas para esta cuenta en la fecha especificada
            var dailyTotal = await _dbContext.TransactionValidations
                .Where(t => t.SourceAccountId == sourceAccountId &&
                            t.ValidationDate >= startOfDay &&
                            t.ValidationDate <= endOfDay &&
                            t.Result == Domain.Enums.ValidationResult.Approved)
                .SumAsync(t => t.TransactionAmount);

            return dailyTotal;
        }
    }
} 