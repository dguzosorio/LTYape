using System.Threading.Tasks;
using AntiFraudService.Domain.Models;

namespace AntiFraudService.Domain.Services
{
    /// <summary>
    /// Interface for services that validate transactions against specific fraud rules
    /// </summary>
    public interface IValidationRuleService
    {
        /// <summary>
        /// Checks if a transaction complies with a specific fraud rule
        /// </summary>
        /// <param name="transaction">The transaction data to validate</param>
        /// <returns>
        /// A validation response indicating whether the transaction is approved or rejected,
        /// and if rejected, the reason for rejection
        /// </returns>
        Task<ValidationResponse> ValidateAsync(TransactionData transaction);
    }
} 