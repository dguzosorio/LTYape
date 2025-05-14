using FluentValidation;
using AntiFraudService.Application.DTOs;
using System;
using System.Linq;

namespace AntiFraudService.Application.Validators
{
    /// <summary>
    /// Validator for transaction validation requests
    /// </summary>
    public class TransactionValidationRequestValidator : AbstractValidator<TransactionValidationRequest>
    {
        // Valores válidos para TransferTypeId basados en el enum TransferType del servicio de transacciones
        private readonly int[] _validTransferTypeIds = { 1, 2, 3 }; // Standard = 1, ATMWithdrawal = 2, International = 3
        
        /// <summary>
        /// Initializes a new instance of the validator
        /// </summary>
        public TransactionValidationRequestValidator()
        {
            RuleFor(x => x.TransactionExternalId)
                .NotEqual(Guid.Empty)
                .WithMessage("Transaction external ID is required.");

            RuleFor(x => x.SourceAccountId)
                .NotEqual(Guid.Empty)
                .WithMessage("Source account ID is required.");

            RuleFor(x => x.TargetAccountId)
                .NotEqual(Guid.Empty)
                .WithMessage("Target account ID is required.");
                
            RuleFor(x => x.TransferTypeId)
                .Must(id => _validTransferTypeIds.Contains(id))
                .WithMessage("Transfer type ID must be a valid value.");

            RuleFor(x => x.Value)
                .GreaterThan(0)
                .WithMessage("Transaction value must be greater than zero.");

            RuleFor(x => x.CreatedAt)
                .NotEmpty()
                .WithMessage("Creation date is required.")
                .Must(date => date <= DateTime.UtcNow)
                .WithMessage("Creation date cannot be in the future.");
        }
    }
} 