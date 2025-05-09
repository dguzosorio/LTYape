using System;
using System.Threading.Tasks;
using AntiFraudService.Application.DTOs;
using AntiFraudService.Domain.Models;
using AntiFraudService.Domain.Services;
using AntiFraudService.Domain.Entities;
using AntiFraudService.Domain.Enums;
using AntiFraudService.Domain.Repositories;

namespace AntiFraudService.Application.Services
{
    /// <summary>
    /// Application service that coordinates fraud validation operations
    /// </summary>
    public class AntiFraudApplicationService : IAntiFraudApplicationService
    {
        private readonly IAntiFraudDomainService _antiFraudDomainService;
        private readonly ITransactionValidationRepository _validationRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AntiFraudApplicationService"/> class
        /// </summary>
        /// <param name="antiFraudDomainService">The anti-fraud domain service</param>
        /// <param name="validationRepository">The repository for accessing validation records</param>
        public AntiFraudApplicationService(
            IAntiFraudDomainService antiFraudDomainService,
            ITransactionValidationRepository validationRepository)
        {
            _antiFraudDomainService = antiFraudDomainService;
            _validationRepository = validationRepository;
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
        /// Validates a transaction against fraud rules
        /// </summary>
        /// <param name="transactionExternalId">The external identifier of the transaction</param>
        /// <param name="sourceAccountId">The identifier of the source account</param>
        /// <param name="value">The monetary value of the transaction</param>
        /// <param name="createdAt">The creation date of the transaction</param>
        /// <returns>The transaction validation result</returns>
        public async Task<TransactionValidation> ValidateTransactionAsync(
            Guid transactionExternalId,
            Guid sourceAccountId,
            decimal value,
            DateTime createdAt)
        {
            var transactionData = new TransactionData
            {
                TransactionExternalId = transactionExternalId,
                SourceAccountId = sourceAccountId,
                Value = value,
                CreatedAt = createdAt
            };
            
            // Execute validation (this method doesn't return a result, it just performs the validation)
            await _antiFraudDomainService.ValidateTransactionAsync(transactionData);
            
            // Get the newly created validation from the repository
            var validation = await _validationRepository.GetByTransactionExternalIdAsync(transactionExternalId);
            
            // If for some reason the validation wasn't found, create a default one
            if (validation == null)
            {
                validation = TransactionValidation.CreateRejected(
                    transactionExternalId,
                    sourceAccountId,
                    value,
                    RejectionReason.Other,
                    "Error processing validation");
                
                await _validationRepository.AddAsync(validation);
            }
            
            return validation;
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
        
        /// <summary>
        /// Determines the domain rejection reason from a string representation
        /// </summary>
        /// <param name="rejectionReasonString">String representation of rejection reason</param>
        /// <returns>Domain rejection reason enum value</returns>
        private RejectionReason DetermineRejectionReason(string rejectionReasonString)
        {
            if (string.IsNullOrEmpty(rejectionReasonString))
                return RejectionReason.None;
                
            if (Enum.TryParse<RejectionReason>(rejectionReasonString, true, out var reason))
                return reason;
                
            return RejectionReason.Other;
        }
    }
} 