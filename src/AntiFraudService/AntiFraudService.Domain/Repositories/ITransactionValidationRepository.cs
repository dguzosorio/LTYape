using System;
using System.Threading.Tasks;
using AntiFraudService.Domain.Entities;

namespace AntiFraudService.Domain.Repositories
{
    /// <summary>
    /// Repository interface for transaction validation data access operations
    /// </summary>
    public interface ITransactionValidationRepository
    {
        /// <summary>
        /// Retrieves a transaction validation by the transaction's external identifier
        /// </summary>
        /// <param name="transactionExternalId">The external ID of the transaction</param>
        /// <returns>The transaction validation if found, or null if not found</returns>
        Task<TransactionValidation> GetByTransactionExternalIdAsync(Guid transactionExternalId);
        
        /// <summary>
        /// Adds a new transaction validation to the repository
        /// </summary>
        /// <param name="validation">The transaction validation to add</param>
        Task AddAsync(TransactionValidation validation);
        
        /// <summary>
        /// Calculates the total amount of transactions for a specific account on a specific date
        /// </summary>
        /// <param name="sourceAccountId">The account identifier</param>
        /// <param name="date">The date for which to calculate the total</param>
        /// <returns>The sum of all transaction amounts for the account on the specified date</returns>
        Task<decimal> GetDailyTransactionAmountForAccountAsync(Guid sourceAccountId, DateTime date);
    }
} 