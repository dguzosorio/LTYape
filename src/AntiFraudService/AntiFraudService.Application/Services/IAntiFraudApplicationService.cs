using System;
using System.Threading.Tasks;
using AntiFraudService.Application.DTOs;
using AntiFraudService.Domain.Entities;

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
        
        /// <summary>
        /// Validates a transaction against fraud rules
        /// </summary>
        /// <param name="transactionExternalId">The external identifier of the transaction</param>
        /// <param name="sourceAccountId">The identifier of the source account</param>
        /// <param name="value">The monetary value of the transaction</param>
        /// <param name="createdAt">The creation date of the transaction</param>
        /// <returns>The transaction validation result</returns>
        Task<TransactionValidation> ValidateTransactionAsync(
            Guid transactionExternalId,
            Guid sourceAccountId,
            decimal value,
            DateTime createdAt);
            
        /// <summary>
        /// Retrieves an existing transaction validation
        /// </summary>
        /// <param name="transactionExternalId">The external identifier of the transaction</param>
        /// <returns>The transaction validation if found, null otherwise</returns>
        /// <remarks>This method is used to check for duplicate validations</remarks>
        Task<TransactionValidation> GetTransactionValidationAsync(Guid transactionExternalId);
    }
} 