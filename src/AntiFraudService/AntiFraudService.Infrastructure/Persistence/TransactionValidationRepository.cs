using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AntiFraudService.Domain.Entities;
using AntiFraudService.Domain.Repositories;

namespace AntiFraudService.Infrastructure.Persistence
{
    /// <summary>
    /// Implementation of transaction validation repository using Entity Framework Core
    /// </summary>
    public class TransactionValidationRepository : ITransactionValidationRepository
    {
        private readonly AntiFraudDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionValidationRepository"/> class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public TransactionValidationRepository(AntiFraudDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves a transaction validation by the transaction's external identifier
        /// </summary>
        /// <param name="transactionExternalId">The external ID of the transaction</param>
        /// <returns>The transaction validation if found, or null if not found</returns>
        public async Task<TransactionValidation> GetByTransactionExternalIdAsync(Guid transactionExternalId)
        {
            return await _dbContext.TransactionValidations
                .FirstOrDefaultAsync(t => t.TransactionExternalId == transactionExternalId);
        }

        /// <summary>
        /// Adds a new transaction validation to the repository
        /// </summary>
        /// <param name="validation">The transaction validation to add</param>
        public async Task AddAsync(TransactionValidation validation)
        {
            await _dbContext.TransactionValidations.AddAsync(validation);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Calculates the total amount of transactions for a specific account on a specific date
        /// </summary>
        /// <param name="sourceAccountId">The account identifier</param>
        /// <param name="date">The date for which to calculate the total</param>
        /// <returns>The sum of all transaction amounts for the account on the specified date</returns>
        public async Task<decimal> GetDailyTransactionAmountForAccountAsync(Guid sourceAccountId, DateTime date)
        {
            // Get start and end of the day
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            // Query approved transactions for this account on the specified date
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