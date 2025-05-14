using System;
using FluentValidation;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Validators
{
    /// <summary>
    /// Validator for transaction retrieval requests
    /// </summary>
    public class GetTransactionRequestValidator : AbstractValidator<GetTransactionRequest>
    {
        /// <summary>
        /// Initializes a new instance of the validator with validation rules
        /// </summary>
        public GetTransactionRequestValidator()
        {
            RuleFor(x => x.TransactionExternalId)
                .NotEqual(Guid.Empty)
                .WithMessage("Transaction external ID is required.");

            RuleFor(x => x.CreatedAt)
                .Must(date => !date.HasValue || date <= DateTime.UtcNow)
                .WithMessage("Creation date cannot be in the future.");
        }
    }
} 