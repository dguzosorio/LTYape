using System.Threading.Tasks;
using AntiFraudService.Domain.Models;

namespace AntiFraudService.Domain.Services
{
    /// <summary>
    /// Interface for communication with the Transaction service
    /// </summary>
    public interface ITransactionService
    {
        /// <summary>
        /// Sends a validation response back to the Transaction service
        /// </summary>
        /// <param name="response">The validation response to send</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SendValidationResponseAsync(ValidationResponse response);
    }
} 