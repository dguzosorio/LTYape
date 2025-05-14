using System;
using System.Threading.Tasks;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Domain.Exceptions;
using TransactionService.Domain.Ports;

namespace TransactionService.Domain.Services
{
    /// <summary>
    /// Implementation of transaction domain service that handles business logic for transactions
    /// </summary>
    public class TransactionDomainService : ITransactionDomainService
    {
        private readonly ITransactionRepositoryPort _transactionRepository;
        private readonly IAntiFraudEventPort _antiFraudEventPort;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionDomainService"/> class
        /// </summary>
        /// <param name="transactionRepository">The transaction repository port</param>
        /// <param name="antiFraudEventPort">The anti-fraud event port</param>
        public TransactionDomainService(
            ITransactionRepositoryPort transactionRepository,
            IAntiFraudEventPort antiFraudEventPort) => 
            (_transactionRepository, _antiFraudEventPort) = (transactionRepository, antiFraudEventPort);

        /// <summary>
        /// Creates a new transaction with the specified details
        /// </summary>
        /// <param name="sourceAccountId">The source account identifier</param>
        /// <param name="targetAccountId">The target account identifier</param>
        /// <param name="transferTypeId">The type of transfer</param>
        /// <param name="value">The monetary value of the transaction</param>
        /// <returns>The created transaction</returns>
        public async Task<Transaction> CreateTransactionAsync(Guid sourceAccountId, Guid targetAccountId, int transferTypeId, decimal value)
        {
            // Value validation is now handled by FluentValidation in the application layer

            var transaction = new Transaction(
                Guid.NewGuid(),
                sourceAccountId,
                targetAccountId,
                transferTypeId,
                value);
            
            await _transactionRepository.addAsync(transaction);
            await _antiFraudEventPort.sendTransactionForValidationAsync(transaction);
            
            return transaction;
        }

        /// <summary>
        /// Retrieves a transaction by its external identifier and creation date
        /// </summary>
        /// <param name="externalId">The external identifier of the transaction</param>
        /// <param name="createdAt">The creation date of the transaction</param>
        /// <returns>The transaction if found, or null if not found</returns>
        public async Task<Transaction?> GetTransactionByExternalIdAndDateAsync(Guid externalId, DateTime createdAt) =>
            await _transactionRepository.getByExternalIdAndDateAsync(externalId, createdAt);

        /// <summary>
        /// Updates the status of a transaction
        /// </summary>
        /// <param name="transaction">The transaction to update</param>
        /// <param name="status">The new status to set</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="TransactionDomainException">
        /// Thrown when the transaction is not in a state that allows status updates
        /// </exception>
        public async Task UpdateTransactionStatusAsync(Transaction transaction, TransactionStatus status)
        {
            if (transaction.Status != TransactionStatus.Pending)
                throw new TransactionDomainException("Only pending transactions can be updated");

            transaction.UpdateStatus(status);
            await _transactionRepository.updateAsync(transaction);
        }
    }
} 