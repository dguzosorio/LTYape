using System;
using System.Threading.Tasks;
using TransactionService.Domain.Entities;

namespace TransactionService.Domain.Repositories
{
    /// <summary>
    /// Repository interface for transaction data access operations
    /// </summary>
    public interface ITransactionRepository
    {
        /// <summary>
        /// Retrieves a transaction by its internal identifier
        /// </summary>
        /// <param name="id">The internal ID of the transaction</param>
        /// <returns>The transaction if found, or null if not found</returns>
        Task<Transaction> GetByIdAsync(int id);
        
        /// <summary>
        /// Retrieves a transaction by its external identifier
        /// </summary>
        /// <param name="externalId">The external ID of the transaction</param>
        /// <returns>The transaction if found, or null if not found</returns>
        Task<Transaction> GetByExternalIdAsync(Guid externalId);
        
        /// <summary>
        /// Adds a new transaction to the repository
        /// </summary>
        /// <param name="transaction">The transaction to add</param>
        Task AddAsync(Transaction transaction);
        
        /// <summary>
        /// Updates an existing transaction in the repository
        /// </summary>
        /// <param name="transaction">The transaction with updated data</param>
        Task UpdateAsync(Transaction transaction);
        
        /// <summary>
        /// Calculates the total sum of transactions for an account on a specific day
        /// </summary>
        /// <param name="accountId">The account identifier</param>
        /// <param name="date">The date for which to calculate the sum</param>
        /// <returns>The sum of transaction values for the specified account and date</returns>
        Task<decimal> GetDailyTransactionSumForAccountAsync(Guid accountId, DateTime date);
    }
} 