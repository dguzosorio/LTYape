using System.Threading.Tasks;
using AntiFraudService.Domain.Enums;
using AntiFraudService.Domain.Models;
using AntiFraudService.Domain.Ports;

namespace AntiFraudService.Domain.Services
{
    /// <summary>
    /// Service that validates transactions against the daily limit rule
    /// </summary>
    public class DailyLimitValidationService : IValidationRuleService
    {
        private const decimal MaximumDailyLimit = 20000;
        private readonly ITransactionValidationRepositoryPort _validationRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DailyLimitValidationService"/> class
        /// </summary>
        /// <param name="validationRepository">The transaction validation repository</param>
        public DailyLimitValidationService(ITransactionValidationRepositoryPort validationRepository)
        {
            _validationRepository = validationRepository;
        }

        /// <summary>
        /// Validates a transaction against the daily limit rule
        /// </summary>
        /// <param name="transaction">The transaction data to validate</param>
        /// <returns>
        /// A validation response indicating whether the transaction is approved or rejected.
        /// If the transaction would cause the daily limit to be exceeded, it will be rejected.
        /// </returns>
        public async Task<ValidationResponse> ValidateAsync(TransactionData transaction)
        {
            // Get the current daily total for this account
            var currentDailyTotal = await _validationRepository.getDailyTransactionAmountForAccountAsync(
                transaction.SourceAccountId, 
                transaction.CreatedAt.Date);

            // Calculate what the new total would be if this transaction is approved
            var newTotal = currentDailyTotal + transaction.Value;

            if (newTotal > MaximumDailyLimit)
            {
                return ValidationResponse.CreateRejected(
                    transaction.TransactionExternalId,
                    RejectionReason.ExceedsDailyLimit,
                    $"Transaction would cause daily limit to be exceeded. Current daily total: {currentDailyTotal}, " +
                    $"Transaction amount: {transaction.Value}, Daily limit: {MaximumDailyLimit}");
            }

            return ValidationResponse.CreateApproved(transaction.TransactionExternalId);
        }
    }
} 