using System;
using System.Linq;
using FluentValidation;
using TransactionService.Application.DTOs;
using TransactionService.Domain.Enums;

namespace TransactionService.Application.Validators
{
    /// <summary>
    /// Validator for transaction creation requests
    /// </summary>
    public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
    {
        /// <summary>
        /// Initializes a new instance of the validator with validation rules
        /// </summary>
        public CreateTransactionRequestValidator()
        {
            RuleFor(x => x.SourceAccountId)
                .NotEqual(Guid.Empty)
                .WithMessage("Source account ID is required.");

            RuleFor(x => x.TargetAccountId)
                .NotEqual(Guid.Empty)
                .WithMessage("Target account ID is required.");

            RuleFor(x => x.TargetAccountId)
                .NotEqual(x => x.SourceAccountId)
                .When(x => x.SourceAccountId != Guid.Empty && x.TargetAccountId != Guid.Empty)
                .WithMessage("Source and target accounts cannot be the same.");

            RuleFor(x => x.TransferTypeId)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Transfer type ID must be a valid value.")
                .Must(id => Enum.IsDefined(typeof(TransferType), id))
                .WithMessage($"Transfer type ID must be a valid value from the TransferType enum. Valid values: {string.Join(", ", Enum.GetNames(typeof(TransferType)))}");

            RuleFor(x => x.Value)
                .GreaterThan(0)
                .WithMessage("Transaction value must be greater than zero.");
        }
    }
} 