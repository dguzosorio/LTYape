using System.Threading.Tasks;
using TransactionService.Domain.Entities;

namespace TransactionService.Domain.Services
{
    /// <summary>
    /// Interface for anti-fraud service communication
    /// </summary>
    public interface IAntiFraudService
    {
        /// <summary>
        /// Sends a transaction to the anti-fraud service for validation
        /// </summary>
        /// <param name="transaction">The transaction to validate</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SendTransactionForValidationAsync(Transaction transaction);
    }
} 