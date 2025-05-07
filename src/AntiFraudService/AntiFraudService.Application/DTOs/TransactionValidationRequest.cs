using System;

namespace AntiFraudService.Application.DTOs
{
    /// <summary>
    /// Data transfer object for receiving transaction validation requests
    /// </summary>
    public class TransactionValidationRequest
    {
        /// <summary>
        /// External unique identifier of the transaction
        /// </summary>
        public Guid TransactionExternalId { get; set; }
        
        /// <summary>
        /// Identifier of the account originating the transaction
        /// </summary>
        public Guid SourceAccountId { get; set; }
        
        /// <summary>
        /// Identifier of the account receiving the transaction
        /// </summary>
        public Guid TargetAccountId { get; set; }
        
        /// <summary>
        /// Type of transfer being executed
        /// </summary>
        public int TransferTypeId { get; set; }
        
        /// <summary>
        /// Monetary value of the transaction
        /// </summary>
        public decimal Value { get; set; }
        
        /// <summary>
        /// Date and time when the transaction was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
} 