using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Repositories;
using TransactionService.Infrastructure.Persistence;

namespace TransactionService.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of transaction repository using Entity Framework Core
    /// </summary>
    public class TransactionRepository : ITransactionRepository
    {
        private readonly TransactionDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRepository"/> class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public TransactionRepository(TransactionDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves a transaction by its internal identifier
        /// </summary>
        /// <param name="id">The internal ID of the transaction</param>
        /// <returns>The transaction if found, or null if not found</returns>
        public async Task<Transaction?> GetByIdAsync(int id)
        {
            return await _dbContext.Transactions.FindAsync(id);
        }

        /// <summary>
        /// Retrieves a transaction by its external identifier
        /// </summary>
        /// <param name="externalId">The external ID of the transaction</param>
        /// <returns>The transaction if found, or null if not found</returns>
        public async Task<Transaction?> GetByExternalIdAsync(Guid externalId)
        {
            return await _dbContext.Transactions
                .FirstOrDefaultAsync(t => t.TransactionExternalId == externalId);
        }

        /// <summary>
        /// Retrieves a transaction by its external identifier and creation date
        /// </summary>
        /// <param name="externalId">The external ID of the transaction</param>
        /// <param name="createdAt">The creation date of the transaction</param>
        /// <returns>The transaction if found, or null if not found</returns>
        public async Task<Transaction?> GetByExternalIdAndDateAsync(Guid externalId, DateTime createdAt)
        {
            return await _dbContext.Transactions
                .FirstOrDefaultAsync(t => 
                    t.TransactionExternalId == externalId && 
                    t.CreatedAt.Date == createdAt.Date);
        }

        /// <summary>
        /// Adds a new transaction to the repository
        /// </summary>
        /// <param name="transaction">The transaction to add</param>
        public async Task AddAsync(Transaction transaction)
        {
            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing transaction in the repository
        /// </summary>
        /// <param name="transaction">The transaction with updated data</param>
        public async Task UpdateAsync(Transaction transaction)
        {
            _dbContext.Transactions.Update(transaction);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Calculates the total sum of transactions for an account on a specific day
        /// </summary>
        /// <param name="accountId">The account identifier</param>
        /// <param name="date">The date for which to calculate the sum</param>
        /// <returns>The sum of transaction values for the specified account and date</returns>
        public async Task<decimal> GetDailyTransactionSumForAccountAsync(Guid accountId, DateTime date)
        {
            return await _dbContext.Transactions
                .Where(t => t.SourceAccountId == accountId 
                    && t.CreatedAt.Date == date.Date)
                .SumAsync(t => t.Value);
        }
    }
} 