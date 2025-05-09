using System;
using System.Threading.Tasks;
using TransactionService.Application.DTOs;
using TransactionService.Domain.Services;

namespace TransactionService.Application.Services
{
    /// <summary>
    /// Implementation of transaction application service that orchestrates transaction operations
    /// </summary>
    public class TransactionApplicationService : ITransactionApplicationService
    {
        private readonly ITransactionDomainService _transactionDomainService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionApplicationService"/> class
        /// </summary>
        /// <param name="transactionDomainService">The transaction domain service</param>
        public TransactionApplicationService(ITransactionDomainService transactionDomainService)
        {
            _transactionDomainService = transactionDomainService;
        }

        /// <summary>
        /// Creates a new transaction with the specified details
        /// </summary>
        /// <param name="request">The transaction creation request</param>
        /// <returns>The created transaction information</returns>
        public async Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request)
        {
            var transaction = await _transactionDomainService.CreateTransactionAsync(
                request.SourceAccountId,
                request.TargetAccountId,
                request.TransferTypeId,
                request.Value);

            return MapTransactionToResponse(transaction);
        }

        /// <summary>
        /// Retrieves transaction information by its external identifier
        /// </summary>
        /// <param name="transactionExternalId">The external identifier of the transaction</param>
        /// <returns>The transaction information if found, or null if not found</returns>
        public async Task<TransactionResponse> GetTransactionAsync(Guid transactionExternalId)
        {
            var transaction = await _transactionDomainService.GetTransactionByExternalIdAsync(transactionExternalId);
            
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