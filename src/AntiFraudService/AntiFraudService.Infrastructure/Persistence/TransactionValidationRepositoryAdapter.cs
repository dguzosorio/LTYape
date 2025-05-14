using AntiFraudService.Domain.Entities;
using AntiFraudService.Domain.Ports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AntiFraudService.Infrastructure.Persistence
{
    /// <summary>
    /// Transaction validation repository adapter using Entity Framework Core
    /// </summary>
    public class TransactionValidationRepositoryAdapter : ITransactionValidationRepositoryPort
    {
        private readonly AntiFraudDbContext _dbContext;
        private readonly ILogger<TransactionValidationRepositoryAdapter> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionValidationRepositoryAdapter"/> class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="logger">The logger</param>
        public TransactionValidationRepositoryAdapter(
            AntiFraudDbContext dbContext,
            ILogger<TransactionValidationRepositoryAdapter> logger
        )
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a transaction validation by the transaction's external identifier
        /// </summary>
        /// <param name="transactionExternalId">External ID of the transaction</param>
        /// <returns>The transaction validation if found, or null if not found</returns>
        public async Task<TransactionValidation> getByTransactionExternalIdAsync(Guid transactionExternalId)
        {
            try
            {
                var validation = await _dbContext.TransactionValidations
                    .FirstOrDefaultAsync(v => v.TransactionExternalId == transactionExternalId);
                
                if (validation == null)
                {
                    _logger.LogWarning(
                        "Validation not found for transaction {TransactionId}",
                        transactionExternalId
                    );
                    throw new KeyNotFoundException($"Validation not found for transaction {transactionExternalId}");
                }
                
                return validation;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(
                    ex,
                    "Error retrieving validation for transaction {TransactionId}: {ErrorMessage}",
                    transactionExternalId,
                    ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// Adds a new transaction validation to the repository
        /// </summary>
        /// <param name="validation">The transaction validation to add</param>
        public async Task addAsync(TransactionValidation validation)
        {
            try
            {
                // Check if the transaction already exists to avoid duplicates
                var existingValidation =
                    await _dbContext.TransactionValidations.FirstOrDefaultAsync(v =>
                        v.TransactionExternalId == validation.TransactionExternalId
                    );

                if (existingValidation != null)
                {
                    _logger.LogWarning(
                        "Duplicate validation attempt received for transaction {TransactionId}. This validation will be ignored.",
                        validation.TransactionExternalId
                    );
                    return; // Do nothing if the validation already exists
                }

                await _dbContext.TransactionValidations.AddAsync(validation);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation(
                    "Transaction validation {TransactionId} successfully stored with result {Result}",
                    validation.TransactionExternalId,
                    validation.Result
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error saving transaction validation {TransactionId}: {ErrorMessage}",
                    validation.TransactionExternalId,
                    ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// Calculates the total transaction amount for a specific account on a specific date
        /// </summary>
        /// <param name="sourceAccountId">The account identifier</param>
        /// <param name="date">The date for which to calculate the total</param>
        /// <returns>The sum of all transaction amounts for the account on the specified date</returns>
        public async Task<decimal> getDailyTransactionAmountForAccountAsync(
            Guid sourceAccountId,
            DateTime date
        )
        {
            // Get start and end of day
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            // Query approved transactions for this account in the specified date
            var dailyTotal = await _dbContext
                .TransactionValidations.Where(t =>
                    t.SourceAccountId == sourceAccountId
                    && t.ValidationDate >= startOfDay
                    && t.ValidationDate <= endOfDay
                    && t.Result == Domain.Enums.ValidationResult.Approved
                )
                .SumAsync(t => t.TransactionAmount);

            return dailyTotal;
        }
    }
}
