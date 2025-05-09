using System;
using TransactionService.Domain.Enums;

namespace TransactionService.Domain.Entities
{
    /// <summary>
    /// Represents a financial transaction between two accounts
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Internal database identifier
        /// </summary>
        public int Id { get; private set; }
        
        /// <summary>
        /// External unique identifier used in API communication
        /// </summary>
        public Guid TransactionExternalId { get; private set; }
        
        /// <summary>
        /// Identifier of the account originating the transaction
        /// </summary>
        public Guid SourceAccountId { get; private set; }
        
        /// <summary>
        /// Identifier of the account receiving the transaction
        /// </summary>
        public Guid TargetAccountId { get; private set; }
        
        /// <summary>
        /// Type of transfer being executed
        /// </summary>
        public int TransferTypeId { get; private set; }
        
        /// <summary>
        /// Monetary value of the transaction
        /// </summary>
        public decimal Value { get; private set; }
        
        /// <summary>
        /// Current status of the transaction (Pending, Approved, Rejected)
        /// </summary>
        public TransactionStatus Status { get; private set; }
        
        /// <summary>
        /// Additional details or notes about the transaction
        /// </summary>
        public string? Notes { get; private set; }
        
        /// <summary>
        /// Date and time when the transaction was created
        /// </summary>
        public DateTime CreatedAt { get; private set; }
        
        /// <summary>
        /// Date and time when the transaction was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; private set; }

        // For EF Core
        private Transaction() { }

        /// <summary>
        /// Creates a new transaction with the specified details
        /// </summary>
        /// <param name="transactionExternalId">External identifier of the transaction</param>
        /// <param name="sourceAccountId">Identifier of the source account</param>
        /// <param name="targetAccountId">Identifier of the target account</param>
        /// <param name="transferTypeId">Identifier of the transfer type</param>
        /// <param name="value">Monetary value of the transaction</param>
        /// <param name="notes">Additional details or notes</param>
        public Transaction(
            Guid transactionExternalId,
            Guid sourceAccountId,
            Guid targetAccountId,
            int transferTypeId,
            decimal value,
            string? notes = null)
        {
            TransactionExternalId = transactionExternalId;
            SourceAccountId = sourceAccountId;
            TargetAccountId = targetAccountId;
            TransferTypeId = transferTypeId;
            Value = value;
            Status = TransactionStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            Notes = notes ?? string.Empty;
        }

        /// <summary>
        /// Updates the status of the transaction
        /// </summary>
        /// <param name="newStatus">The new status to set</param>
        public void UpdateStatus(TransactionStatus newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the notes of the transaction
        /// </summary>
        /// <param name="notes">New notes to set</param>
        public void UpdateNotes(string? notes)
        {
            Notes = notes;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the transaction as approved
        /// </summary>
        public void Approve()
        {
            UpdateStatus(TransactionStatus.Approved);
        }

        /// <summary>
        /// Marks the transaction as rejected with optional notes
        /// </summary>
        /// <param name="notes">Optional notes explaining the rejection</param>
        public void Reject(string? notes = null)
        {
            UpdateStatus(TransactionStatus.Rejected);
            if (!string.IsNullOrEmpty(notes))
            {
                UpdateNotes(notes);
            }
        }

        /// <summary>
        /// Marks the transaction as completed
        /// </summary>
        public void Complete()
        {
            UpdateStatus(TransactionStatus.Completed);
        }
    }
} 