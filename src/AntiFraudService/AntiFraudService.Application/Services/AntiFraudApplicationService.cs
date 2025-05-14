using System;
using System.Threading.Tasks;
using AntiFraudService.Application.DTOs;
using AntiFraudService.Domain.Models;
using AntiFraudService.Domain.Services;
using AntiFraudService.Domain.Entities;
using AntiFraudService.Domain.Enums;
using AntiFraudService.Domain.Ports;

namespace AntiFraudService.Application.Services
{
    /// <summary>
    /// Application service that coordinates fraud validation operations
    /// </summary>
    public class AntiFraudApplicationService : IAntiFraudApplicationService
    {
        private readonly IAntiFraudDomainService _antiFraudDomainService;
        private readonly ITransactionValidationRepositoryPort _validationRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AntiFraudApplicationService"/> class
        /// </summary>
        /// <param name="antiFraudDomainService">The anti-fraud domain service</param>
        /// <param name="validationRepository">The repository port for accessing validation records</param>
        public AntiFraudApplicationService(
            IAntiFraudDomainService antiFraudDomainService,
            ITransactionValidationRepositoryPort validationRepository)
        {
            _antiFraudDomainService = antiFraudDomainService;
            _validationRepository = validationRepository;
        }

        /// <summary>
        /// Processes a transaction validation request
        /// </summary>
        /// <param name="request">The transaction validation request</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task ProcessTransactionValidationRequestAsync(AntiFraudService.Application.DTOs.TransactionValidationRequest request)
        {
            // Map DTO to domain model
            var transactionData = MapRequestToTransactionData(request);
            
            // Process the validation through the domain service
            await _antiFraudDomainService.ValidateTransactionAsync(transactionData);
        }

        /// <summary>
        /// Retrieves an existing transaction validation
        /// </summary>
        /// <param name="transactionExternalId">The external identifier of the transaction</param>
        /// <returns>The transaction validation if found, null otherwise</returns>
        public async Task<TransactionValidation> GetTransactionValidationAsync(Guid transactionExternalId)
        {
            try
            {
                return await _validationRepository.getByTransactionExternalIdAsync(transactionExternalId);
            }
            catch (KeyNotFoundException)
            {
                // Propagate the exception so the controller can handle it
                throw;
            }
            catch (Exception ex)
            {
                // Log the error but rethrow to allow the controller to handle it
                Console.WriteLine($"Error retrieving validation for transaction {transactionExternalId}: {ex.Message}");
                throw;
            }
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
            TransactionValidation validation;
            try 
            {
                validation = await _validationRepository.getByTransactionExternalIdAsync(transactionExternalId);
            }
            catch (KeyNotFoundException)
            {
                validation = TransactionValidation.CreateRejected(
                    transactionExternalId,
                    sourceAccountId,
                    value,
                    RejectionReason.Other,
                    "Error processing the validation");
                
                await _validationRepository.addAsync(validation);
            }
            
            return validation;
        }

        /// <summary>
        /// Maps a transaction validation request DTO to a transaction data domain model
        /// </summary>
        /// <param name="request">The transaction validation request DTO</param>
        /// <returns>The transaction data domain model</returns>
        private static TransactionData MapRequestToTransactionData(AntiFraudService.Application.DTOs.TransactionValidationRequest request)
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