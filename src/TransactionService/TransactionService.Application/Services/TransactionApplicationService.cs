using System;
using System.Threading.Tasks;
using TransactionService.Application.DTOs;
using TransactionService.Application.Exceptions;
using TransactionService.Domain.Enums;
using TransactionService.Domain.Services;
using FluentValidation;

namespace TransactionService.Application.Services
{
    /// <summary>
    /// Implementation of transaction application service that orchestrates transaction operations
    /// </summary>
    public class TransactionApplicationService : ITransactionApplicationService
    {
        private readonly ITransactionDomainService _transactionDomainService;
        private readonly IValidator<CreateTransactionRequest> _createTransactionValidator;
        private readonly IValidator<GetTransactionRequest> _getTransactionValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionApplicationService"/> class
        /// </summary>
        /// <param name="transactionDomainService">The transaction domain service</param>
        /// <param name="createTransactionValidator">Validator for transaction creation requests</param>
        /// <param name="getTransactionValidator">Validator for transaction retrieval requests</param>
        public TransactionApplicationService(
            ITransactionDomainService transactionDomainService,
            IValidator<CreateTransactionRequest> createTransactionValidator,
            IValidator<GetTransactionRequest> getTransactionValidator)
        {
            _transactionDomainService = transactionDomainService;
            _createTransactionValidator = createTransactionValidator;
            _getTransactionValidator = getTransactionValidator;
        }

        /// <summary>
        /// Creates a new transaction with the specified details
        /// </summary>
        /// <param name="request">The transaction creation request</param>
        /// <returns>The created transaction information</returns>
        public async Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request)
        {
            // Validate request data using FluentValidation
            await _createTransactionValidator.ValidateAndThrowAsync(request);

            // Validate transfer type using Enum.IsDefined (domain-specific rule)
            if (!Enum.IsDefined(typeof(TransferType), request.TransferTypeId))
            {
                throw new InvalidRequestException($"Invalid transfer type. Valid types are: {string.Join(", ", Enum.GetNames(typeof(TransferType)))}");
            }

            var transaction = await _transactionDomainService.CreateTransactionAsync(
                request.SourceAccountId,
                request.TargetAccountId,
                request.TransferTypeId,
                request.Value);

            return MapTransactionToResponse(transaction);
        }

        /// <summary>
        /// Retrieves transaction information by its external identifier and creation date
        /// </summary>
        /// <param name="request">The transaction retrieval request</param>
        /// <returns>The transaction information if found, or null if not found</returns>
        public async Task<TransactionResponse?> GetTransactionAsync(GetTransactionRequest request)
        {
            // Validate request data using FluentValidation
            await _getTransactionValidator.ValidateAndThrowAsync(request);

            var transaction = await _transactionDomainService.GetTransactionByExternalIdAndDateAsync(
                request.TransactionExternalId,
                request.CreatedAt ?? DateTime.UtcNow);

            if (transaction == null)
                return null;

            return MapTransactionToResponse(transaction);
        }

        /// <summary>
        /// Maps a domain transaction entity to a transaction response DTO
        /// </summary>
        /// <param name="transaction">The domain transaction entity</param>
        /// <returns>The transaction response DTO</returns>
        private TransactionResponse MapTransactionToResponse(Domain.Entities.Transaction transaction)
        {
            return new TransactionResponse
            {
                TransactionExternalId = transaction.TransactionExternalId,
                SourceAccountId = transaction.SourceAccountId,
                TargetAccountId = transaction.TargetAccountId,
                TransferTypeId = transaction.TransferTypeId,
                Value = transaction.Value,
                Status = transaction.Status.ToString(),
                CreatedAt = transaction.CreatedAt,
                UpdatedAt = transaction.UpdatedAt
            };
        }
    }
} 