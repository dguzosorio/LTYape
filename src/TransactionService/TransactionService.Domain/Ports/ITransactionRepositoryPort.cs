using System;
using System.Threading.Tasks;
using TransactionService.Domain.Entities;

namespace TransactionService.Domain.Ports
{
    /// <summary>
    /// Puerto para operaciones de acceso a datos de transacciones
    /// </summary>
    public interface ITransactionRepositoryPort
    {
        /// <summary>
        /// Recupera una transacción por su identificador externo y fecha de creación
        /// </summary>
        /// <param name="externalId">El ID externo de la transacción</param>
        /// <param name="createdAt">La fecha de creación de la transacción</param>
        /// <returns>La transacción si se encuentra, o null si no se encuentra</returns>
        Task<Transaction?> getByExternalIdAndDateAsync(Guid externalId, DateTime createdAt);
        
        /// <summary>
        /// Añade una nueva transacción al repositorio
        /// </summary>
        /// <param name="transaction">La transacción a añadir</param>
        Task addAsync(Transaction transaction);
        
        /// <summary>
        /// Actualiza una transacción existente en el repositorio
        /// </summary>
        /// <param name="transaction">La transacción con datos actualizados</param>
        Task updateAsync(Transaction transaction);
        
        /// <summary>
        /// Calcula la suma total de transacciones para una cuenta en un día específico
        /// </summary>
        /// <param name="accountId">El identificador de la cuenta</param>
        /// <param name="date">La fecha para la cual calcular la suma</param>
        /// <returns>La suma de valores de transacción para la cuenta y fecha especificadas</returns>
        Task<decimal> getDailyTransactionSumForAccountAsync(Guid accountId, DateTime date);
    }
} 