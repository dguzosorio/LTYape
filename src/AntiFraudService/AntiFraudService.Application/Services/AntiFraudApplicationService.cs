using System.Threading.Tasks;
using AntiFraudService.Application.DTOs;
using AntiFraudService.Domain.Models;
using AntiFraudService.Domain.Services;

namespace AntiFraudService.Application.Services
{
    /// <summary>
    /// Application service that coordinates fraud validation operations
    /// </summary>
    public class AntiFraudApplicationService : IAntiFraudApplicationService
    {
        private readonly IAntiFraudDomainService _antiFraudDomainService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AntiFraudApplicationService"/> class
        /// </summary>
        /// <param name="antiFraudDomainService">The anti-fraud domain service</param>
        public AntiFraudApplicationService(IAntiFraudDomainService antiFraudDomainService)
        {
            _antiFraudDomainService = antiFraudDomainService;
        }

        /// <summary>
        /// Processes a transaction validation request
        /// </summary>
        /// <param name="request">The transaction validation request</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task ProcessTransactionValidationRequestAsync(TransactionValidationRequest request)
        {
            // Map DTO to domain model
            var transactionData = MapToTransactionData(request);
            
            // Process the validation through the domain service
            await _antiFraudDomainService.ValidateTransactionAsync(transactionData);
        }

        /// <summary>
        /// Maps a transaction validation request DTO to a transaction data domain model
        /// </summary>
        /// <param name="request">The transaction validation request DTO</param>
        /// <returns>The transaction data domain model</returns>
        private static TransactionData MapToTransactionData(TransactionValidationRequest request)
        {
            return new TransactionData
            {
                TransactionExternalId = request.TransactionExternalId,
                SourceAccountId = request.SourceAccountId,
                TargetAccountId = request.TargetAccountId,
                TransferTypeId = request.TransferTypeId,
                Value = request.Value,
                CreatedAt = request.CreatedAt
            };
        }
    }
} 