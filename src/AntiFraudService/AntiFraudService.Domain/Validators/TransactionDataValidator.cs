using System;
using FluentValidation;
using AntiFraudService.Domain.Models;

namespace AntiFraudService.Domain.Validators
{
    /// <summary>
    /// Validator for transaction data used in the domain layer
    /// </summary>
    public class TransactionDataValidator : AbstractValidator<TransactionData>
    {
        /// <summary>
        /// Initializes a new instance of the validator with validation rules
        /// </summary>
        public TransactionDataValidator()
        {
            RuleFor(x => x.TransactionExternalId)
                .NotEqual(Guid.Empty)
                .WithMessage("Transaction external ID is required");

            RuleFor(x => x.SourceAccountId)
                .NotEqual(Guid.Empty)
                .WithMessage("Source account ID is required");

            RuleFor(x => x.Value)
                .GreaterThan(0)
                .WithMessage("Transaction value must be greater than zero");

            RuleFor(x => x.CreatedAt)
                .NotEmpty()
                .WithMessage("Creation date is required")
                .Must(date => date <= DateTime.UtcNow)
                .WithMessage("Creation date cannot be in the future");
        }
    }
} 