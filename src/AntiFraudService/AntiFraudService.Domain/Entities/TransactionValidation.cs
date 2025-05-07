using System;
using AntiFraudService.Domain.Enums;

namespace AntiFraudService.Domain.Entities
{
    /// <summary>
    /// Represents the result of validating a transaction against fraud rules
    /// </summary>
    public class TransactionValidation
    {
        /// <summary>
        /// Internal database identifier
        /// </summary>
        public int Id { get; private set; }
        
        /// <summary>
        /// External identifier of the validated transaction
        /// </summary>
        public Guid TransactionExternalId { get; private set; }
        
        /// <summary>
        /// Identifier of the account originating the transaction
        /// </summary>
        public Guid SourceAccountId { get; private set; }
        
        /// <summary>
        /// Monetary value of the transaction
        /// </summary>
        public decimal TransactionAmount { get; private set; }
        
        /// <summary>
        /// Date and time when the transaction was validated
        /// </summary>
        public DateTime ValidationDate { get; private set; }
        
        /// <summary>
        /// Result of the validation (Approved or Rejected)
        /// </summary>
        public ValidationResult Result { get; private set; }
        
        /// <summary>
        /// Reason for rejection if the transaction was rejected
        /// </summary>
        public RejectionReason RejectionReason { get; private set; }
        
        /// <summary>
        /// Additional details or notes about the validation
        /// </summary>
        public string Notes { get; private set; }

        // For EF Core
        private TransactionValidation() { }

        /// <summary>
        /// Creates a new transaction validation with the specified details
        /// </summary>
        /// <param name="transactionExternalId">External identifier of the transaction</param>
        /// <param name="sourceAccountId">Identifier of the source account</param>
        /// <param name="transactionAmount">Monetary value of the transaction</param>
        /// <param name="result">Result of the validation</param>
        /// <param name="rejectionReason">Reason for rejection, if applicable</param>
        /// <param name="notes">Additional details or notes</param>
        public TransactionValidation(
            Guid transactionExternalId,
            Guid sourceAccountId,
            decimal transactionAmount,
            ValidationResult result,
            RejectionReason rejectionReason = RejectionReason.None,
            string notes = null)
        {
            TransactionExternalId = transactionExternalId;
            SourceAccountId = sourceAccountId;
            TransactionAmount = transactionAmount;
            ValidationDate = DateTime.UtcNow;
            Result = result;
            RejectionReason = rejectionReason;
            Notes = notes;
        }

        /// <summary>
        /// Creates an approved transaction validation
        /// </summary>
        /// <param name="transactionExternalId">External identifier of the transaction</param>
        /// <param name="sourceAccountId">Identifier of the source account</param>
        /// <param name="transactionAmount">Monetary value of the transaction</param>
        /// <returns>A new transaction validation with approved status</returns>
        public static TransactionValidation CreateApproved(
            Guid transactionExternalId,
            Guid sourceAccountId,
            decimal transactionAmount)
        {
            return new TransactionValidation(
                transactionExternalId,
                sourceAccountId,
                transactionAmount,
                ValidationResult.Approved);
        }

        /// <summary>
        /// Creates a rejected transaction validation
        /// </summary>
        /// <param name="transactionExternalId">External identifier of the transaction</param>
        /// <param name="sourceAccountId">Identifier of the source account</param>
        /// <param name="transactionAmount">Monetary value of the transaction</param>
        /// <param name="reason">Reason for rejection</param>
        /// <param name="notes">Additional details or notes</param>
        /// <returns>A new transaction validation with rejected status</returns>
        public static TransactionValidation CreateRejected(
            Guid transactionExternalId,
            Guid sourceAccountId,
            decimal transactionAmount,
            RejectionReason reason,
            string notes = null)
        {
            return new TransactionValidation(
                transactionExternalId,
                sourceAccountId,
                transactionAmount,
                ValidationResult.Rejected,
                reason,
                notes);
        }
    }
} 