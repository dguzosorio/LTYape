using System;
using System.Threading.Tasks;
using AntiFraudService.Domain.Entities;

namespace AntiFraudService.Domain.Ports
{
    /// <summary>
    /// Puerto para operaciones de acceso a datos de validación de transacciones
    /// </summary>
    public interface ITransactionValidationRepositoryPort
    {
        /// <summary>
        /// Recupera una validación de transacción por el identificador externo de la transacción
        /// </summary>
        /// <param name="transactionExternalId">ID externo de la transacción</param>
        /// <returns>La validación de transacción si se encuentra, o null si no se encuentra</returns>
        Task<TransactionValidation> getByTransactionExternalIdAsync(Guid transactionExternalId);
        
        /// <summary>
        /// Añade una nueva validación de transacción al repositorio
        /// </summary>
        /// <param name="validation">La validación de transacción a añadir</param>
        Task addAsync(TransactionValidation validation);
        
        /// <summary>
        /// Calcula el monto total de transacciones para una cuenta específica en una fecha específica
        /// </summary>
        /// <param name="sourceAccountId">El identificador de la cuenta</param>
        /// <param name="date">La fecha para la cual calcular el total</param>
        /// <returns>La suma de todos los montos de transacciones para la cuenta en la fecha especificada</returns>
        Task<decimal> getDailyTransactionAmountForAccountAsync(Guid sourceAccountId, DateTime date);
    }
} 