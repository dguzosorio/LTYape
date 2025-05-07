using System.Threading.Tasks;
using AntiFraudService.Application.DTOs;

namespace AntiFraudService.Application.Services
{
    /// <summary>
    /// Application service interface for fraud validation operations
    /// </summary>
    public interface IAntiFraudApplicationService
    {
        /// <summary>
        /// Processes a transaction validation request
        /// </summary>
        /// <param name="request">The transaction validation request</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task ProcessTransactionValidationRequestAsync(TransactionValidationRequest request);
    }
} 