using System;
using AntiFraudService.Domain.Enums;

namespace AntiFraudService.Domain.Models
{
    /// <summary>
    /// Represents a validation response sent back to the Transaction service
    /// </summary>
    public class ValidationResponse
    {
        /// <summary>
        /// External identifier of the validated transaction
        /// </summary>
        public Guid TransactionExternalId { get; set; }
        
        /// <summary>
        /// Result of the validation (Approved or Rejected)
        /// </summary>
        public ValidationResult Result { get; set; }
        
        /// <summary>
        /// Reason for rejection if the transaction was rejected
        /// </summary>
        public RejectionReason RejectionReason { get; set; }
        
        /// <summary>
        /// Additional details or notes about the validation
        /// </summary>
        public string? Notes { get; set; }
        
        /// <summary>
        /// Date and time when the validation response was created
        /// </summary>
        public DateTime ResponseDate { get; set; }

        /// <summary>
        /// Creates a new validation response with current date/time
        /// </summary>
        public ValidationResponse()
        {
            ResponseDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a new approved validation response
        /// </summary>
        /// <param name="transactionExternalId">External identifier of the transaction</param>
        /// <returns>A new validation response with approved status</returns>
        public static ValidationResponse CreateApproved(Guid transactionExternalId)
        {
            return new ValidationResponse
            {
                TransactionExternalId = transactionExternalId,
                Result = ValidationResult.Approved,
                RejectionReason = RejectionReason.None
            };
        }

        /// <summary>
        /// Creates a new rejected validation response
        /// </summary>
        /// <param name="transactionExternalId">External identifier of the transaction</param>
        /// <param name="reason">Reason for rejection</param>
        /// <param name="notes">Additional details or notes</param>
        /// <returns>A new validation response with rejected status</returns>
        public static ValidationResponse CreateRejected(
            Guid transactionExternalId,
            RejectionReason reason,
            string? notes = null)
        {
            return new ValidationResponse
            {
                TransactionExternalId = transactionExternalId,
                Result = ValidationResult.Rejected,
                RejectionReason = reason,
                Notes = notes ?? $"Transaction rejected: {reason}"
            };
        }
    }
} 