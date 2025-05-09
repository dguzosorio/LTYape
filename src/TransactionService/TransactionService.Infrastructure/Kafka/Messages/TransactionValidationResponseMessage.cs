using System;
using TransactionService.Domain.Enums;

namespace TransactionService.Infrastructure.Kafka.Messages
{
    /// <summary>
    /// Message for transaction validation response
    /// </summary>
    public class TransactionValidationResponseMessage
    {
        /// <summary>
        /// Gets or sets the transaction external identifier
        /// </summary>
        public Guid TransactionExternalId { get; set; }
        
        /// <summary>
        /// Gets or sets whether the transaction is valid
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// Gets or sets the rejection reason if the transaction is invalid
        /// </summary>
        public string RejectionReason { get; set; }
        
        /// <summary>
        /// Gets or sets additional notes about the validation
        /// </summary>
        public string Notes { get; set; }
    }
} 