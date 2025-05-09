using System;
using System.Threading.Tasks;
using TransactionService.Domain.Entities;

namespace TransactionService.Domain.Services
{
    /// <summary>
    /// Service interface for transaction domain operations
    /// </summary>
    public interface ITransactionDomainService
    {
        /// <summary>
        /// Creates a new transaction with the specified details
        /// </summary>
        /// <param name="sourceAccountId">The source account identifier</param>
        /// <param name="targetAccountId">The target account identifier</param>
        /// <param name="transferTypeId">The type of transfer</param>
        /// <param name="value">The monetary value of the transaction</param>
        /// <returns>The created transaction</returns>
        /// <exception cref="TransactionService.Domain.Exceptions.TransactionDomainException">
        /// Thrown when the transaction value is invalid
        /// </exception>
        Task<Transaction> CreateTransactionAsync(Guid sourceAccountId, Guid targetAccountId, int transferTypeId, decimal value);
        
        /// <summary>
        /// Retrieves a transaction by its external identifier
        /// </summary>
        /// <param name="externalId">The external identifier of the transaction</param>
        /// <returns>The transaction if found, or null if not found</returns>
        Task<Transaction> GetTransactionByExternalIdAsync(Guid externalId);
        
        /// <summary>
        /// Updates the status of a transaction
        /// </summary>
        /// <param name="transaction">The transaction to update</param>
        /// <param name="status">The new status to set</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="TransactionService.Domain.Exceptions.TransactionDomainException">
        /// Thrown when the transaction is not in a state that allows status updates
        /// </exception>
        Task UpdateTransactionStatusAsync(Transaction transaction, Enums.TransactionStatus status);
    }
} 