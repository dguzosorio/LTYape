using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntiFraudService.Domain.Entities;
using AntiFraudService.Domain.Enums;
using AntiFraudService.Domain.Models;
using AntiFraudService.Domain.Ports;
using AntiFraudService.Domain.Validators;
using FluentValidation;

namespace AntiFraudService.Domain.Services
{
    /// <summary>
    /// Domain service that orchestrates transaction validation against fraud rules
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AntiFraudDomainService"/> class
    /// </remarks>
    /// <param name="validationRules">The collection of validation rules to apply</param>
    /// <param name="transactionEventPort">The transaction event port for sending responses</param>
    /// <param name="validationRepository">The repository for storing validation results</param>
    /// <param name="validator">The validator for transaction data</param>
    public class AntiFraudDomainService : IAntiFraudDomainService
    {
        private readonly IEnumerable<IValidationRuleService> _validationRules;
        private readonly ITransactionEventPort _transactionEventPort;
        private readonly ITransactionValidationRepositoryPort _validationRepository;
        private readonly IValidator<TransactionData> _validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AntiFraudDomainService"/> class
        /// </summary>
        /// <param name="validationRules">The collection of validation rules to apply</param>
        /// <param name="transactionEventPort">The transaction event port for sending responses</param>
        /// <param name="validationRepository">The repository for storing validation results</param>
        /// <param name="validator">The validator for transaction data</param>
        public AntiFraudDomainService(
            IEnumerable<IValidationRuleService> validationRules,
            ITransactionEventPort transactionEventPort,
            ITransactionValidationRepositoryPort validationRepository,
            IValidator<TransactionData> validator)
        {
            _validationRules = validationRules;
            _transactionEventPort = transactionEventPort;
            _validationRepository = validationRepository;
            _validator = validator;
        }

        /// <summary>
        /// Validates a transaction against all fraud rules
        /// </summary>
        /// <param name="transaction">The transaction data to validate</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task ValidateTransactionAsync(TransactionData transaction)
        {
            // Validate transaction data before processing
            ValidateTransactionData(transaction);
            
            // Apply all validation rules
            var validationResults = await Task.WhenAll(
                _validationRules.Select(rule => rule.ValidateAsync(transaction)));

            // Find the first rejected result, if any
            var rejectedResult = validationResults.FirstOrDefault(r => r.Result == ValidationResult.Rejected);
            
            // Create the final validation response
            ValidationResponse finalResponse;
            
            if (rejectedResult != null)
            {
                // Transaction is rejected
                finalResponse = rejectedResult;
            }
            else
            {
                // Transaction is approved
                finalResponse = ValidationResponse.CreateApproved(transaction.TransactionExternalId);
            }

            // Create and save the validation record
            var validation = finalResponse.Result == ValidationResult.Approved
                ? TransactionValidation.CreateApproved(
                    transaction.TransactionExternalId,
                    transaction.SourceAccountId,
                    transaction.Value)
                : TransactionValidation.CreateRejected(
                    transaction.TransactionExternalId,
                    transaction.SourceAccountId,
                    transaction.Value,
                    finalResponse.RejectionReason,
                    finalResponse.Notes);

            // Save the validation result
            await _validationRepository.addAsync(validation);

            // Send the response back to the Transaction service
            await _transactionEventPort.sendValidationResponseAsync(finalResponse);
        }
        
        /// <summary>
        /// Validates that transaction data is correct and complete using FluentValidation
        /// </summary>
        /// <param name="transaction">The transaction data to validate</param>
        /// <exception cref="ArgumentNullException">If the transaction is null</exception>
        /// <exception cref="ValidationException">If the transaction fails validation rules</exception>
        private void ValidateTransactionData(TransactionData transaction)
        {
            if (transaction is null)
            {
                throw new ArgumentNullException(nameof(transaction), "Transaction data cannot be null");
            }
            
            // Validate using FluentValidation
            _validator.ValidateAndThrow(transaction);
        }
    }
} 