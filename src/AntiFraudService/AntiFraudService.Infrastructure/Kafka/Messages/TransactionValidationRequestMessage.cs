using System;

namespace AntiFraudService.Infrastructure.Kafka.Messages
{
    /// <summary>
    /// Message for transaction validation request
    /// </summary>
    public class TransactionValidationRequestMessage
    {
        /// <summary>
        /// Gets or sets the transaction external identifier
        /// </summary>
        public Guid TransactionExternalId { get; set; }
        
        /// <summary>
        /// Gets or sets the source account identifier
        /// </summary>
        public Guid SourceAccountId { get; set; }
        
        /// <summary>
        /// Gets or sets the target account identifier
        /// </summary>
        public Guid TargetAccountId { get; set; }
        
        /// <summary>
        /// Gets or sets the transfer type identifier
        /// </summary>
        public int TransferTypeId { get; set; }
        
        /// <summary>
        /// Gets or sets the transaction value
        /// </summary>
        public decimal Value { get; set; }
        
        /// <summary>
        /// Gets or sets the transaction creation date
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
} 