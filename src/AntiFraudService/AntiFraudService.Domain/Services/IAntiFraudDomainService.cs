using System.Threading.Tasks;
using AntiFraudService.Domain.Models;

namespace AntiFraudService.Domain.Services
{
    /// <summary>
    /// Domain service interface for fraud validation operations
    /// </summary>
    public interface IAntiFraudDomainService
    {
        /// <summary>
        /// Validates a transaction against all fraud rules
        /// </summary>
        /// <param name="transaction">The transaction data to validate</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task ValidateTransactionAsync(TransactionData transaction);
    }
} 