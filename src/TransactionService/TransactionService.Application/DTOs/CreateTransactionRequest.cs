
using System;

namespace TransactionService.Application.DTOs
{
    /// <summary>
    /// Data transfer object for creating a new financial transaction
    /// </summary>
    public class CreateTransactionRequest
    {
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
    }
} 