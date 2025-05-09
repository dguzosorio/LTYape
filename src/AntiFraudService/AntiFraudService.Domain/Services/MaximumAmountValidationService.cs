using System.Threading.Tasks;
using AntiFraudService.Domain.Enums;
using AntiFraudService.Domain.Models;

namespace AntiFraudService.Domain.Services
{
    /// <summary>
    /// Service that validates transactions against the maximum amount rule
    /// </summary>
    public class MaximumAmountValidationService : IValidationRuleService
    {
        private const decimal MaximumTransactionAmount = 2000;

        /// <summary>
        /// Validates a transaction against the maximum amount rule
        /// </summary>
        /// <param name="transaction">The transaction data to validate</param>
        /// <returns>
        /// A validation response indicating whether the transaction is approved or rejected.
        /// If the transaction amount exceeds the maximum allowed amount, it will be rejected.
        /// </returns>
        public Task<ValidationResponse> ValidateAsync(TransactionData transaction)
        {
            ValidationResponse response;

            if (transaction.Value > MaximumTransactionAmount)
            {
                response = ValidationResponse.CreateRejected(
                    transaction.TransactionExternalId,
                    RejectionReason.ExceedsMaximumAmount,
                    $"Transaction amount ({transaction.Value}) exceeds the maximum allowed amount ({MaximumTransactionAmount})");
            }
            else
            {
                response = ValidationResponse.CreateApproved(transaction.TransactionExternalId);
            }

            return Task.FromResult(response);
        }
    }
} 