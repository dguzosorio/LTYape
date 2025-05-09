using System;

namespace TransactionService.Application.DTOs
{
    /// <summary>
    /// Data transfer object for retrieving a transaction
    /// </summary>
    public class GetTransactionRequest
    {
        /// <summary>
        /// External unique identifier of the transaction
        /// </summary>
        public Guid TransactionExternalId { get; set; }
        
        /// <summary>
        /// Date when the transaction was created
        /// </summary>
        public DateTime? CreatedAt { get; set; }
    }
} 