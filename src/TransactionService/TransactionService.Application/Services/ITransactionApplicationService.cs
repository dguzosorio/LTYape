using System;
using System.Threading.Tasks;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Services
{
    /// <summary>
    /// Service interface for transaction application layer operations
    /// </summary>
    public interface ITransactionApplicationService
    {
        /// <summary>
        /// Creates a new transaction with the specified details
        /// </summary>
        /// <param name="request">The transaction creation request</param>
        /// <returns>The created transaction information</returns>
        Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request);
        
        /// <summary>
        /// Retrieves transaction information by its external identifier and creation date
        /// </summary>
        /// <param name="request">The transaction retrieval request</param>
        /// <returns>The transaction information if found, or null if not found</returns>
        Task<TransactionResponse?> GetTransactionAsync(GetTransactionRequest request);
    }
} 