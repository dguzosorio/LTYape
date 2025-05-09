using System;

namespace TransactionService.Application.DTOs
{
    /// <summary>
    /// Data transfer object containing transaction information returned by the API
    /// </summary>
    public class TransactionResponse
    {
        /// <summary>
        /// External unique identifier of the transaction
        /// </summary>
        public Guid TransactionExternalId { get; set; }
        
        /// <summary>
        /// Identifier of the account that originated the transaction
        /// </summary>
        public Guid SourceAccountId { get; set; }
        
        /// <summary>
        /// Identifier of the account that received the transaction
        /// </summary>
        public Guid TargetAccountId { get; set; }
        
        /// <summary>
        /// Type of transfer that was executed
        /// </summary>
        public int TransferTypeId { get; set; }
        
        /// <summary>
        /// Monetary value of the transaction
        /// </summary>
        public decimal Value { get; set; }
        
        /// <summary>
        /// Current status of the transaction (Pending, Approved, Rejected)
        /// </summary>
        public required string Status { get; set; }
        
        /// <summary>
        /// Date and time when the transaction was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Date and time when the transaction was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
} 