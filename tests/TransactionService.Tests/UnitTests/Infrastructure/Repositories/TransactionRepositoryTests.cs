using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure.Persistence;
using TransactionService.Infrastructure.Repositories;
using Xunit;

namespace TransactionService.Tests.UnitTests.Infrastructure.Repositories
{
    public class TransactionRepositoryTests : IDisposable
    {
        private readonly TransactionDbContext _dbContext;
        private readonly TransactionRepository _repository;

        public TransactionRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<TransactionDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new TransactionDbContext(options);
            _repository = new TransactionRepository(_dbContext);
        }

        [Fact]
        public async Task GetByExternalIdAndDateAsync_WhenTransactionExists_ReturnsTransaction()
        {
            // Arrange
            var transaction = new Transaction(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                100
            );

            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByExternalIdAndDateAsync(
                transaction.TransactionExternalId,
                transaction.CreatedAt);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transaction.TransactionExternalId, result.TransactionExternalId);
            Assert.Equal(transaction.CreatedAt.Date, result.CreatedAt.Date);
        }

        [Fact]
        public async Task GetByExternalIdAndDateAsync_WhenTransactionDoesNotExist_ReturnsNull()
        {
            // Arrange
            var externalId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;

            // Act
            var result = await _repository.GetByExternalIdAndDateAsync(externalId, createdAt);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByExternalIdAndDateAsync_WhenTransactionExistsButDateDoesNotMatch_ReturnsNull()
        {
            // Arrange
            var transaction = new Transaction(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                100
            );

            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByExternalIdAndDateAsync(
                transaction.TransactionExternalId,
                transaction.CreatedAt.AddDays(1));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_AddsTransactionToDatabase()
        {
            // Arrange
            var transaction = new Transaction(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                100
            );

            // Act
            await _repository.AddAsync(transaction);

            // Assert
            var savedTransaction = await _dbContext.Transactions
                .FirstOrDefaultAsync(t => t.TransactionExternalId == transaction.TransactionExternalId);

            Assert.NotNull(savedTransaction);
            Assert.Equal(transaction.SourceAccountId, savedTransaction.SourceAccountId);
            Assert.Equal(transaction.TargetAccountId, savedTransaction.TargetAccountId);
            Assert.Equal(transaction.TransferTypeId, savedTransaction.TransferTypeId);
            Assert.Equal(transaction.Value, savedTransaction.Value);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesTransactionInDatabase()
        {
            // Arrange
            var transaction = new Transaction(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                100
            );

            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();

            // Act
            transaction.UpdateStatus(Domain.Enums.TransactionStatus.Approved);
            await _repository.UpdateAsync(transaction);

            // Assert
            var updatedTransaction = await _dbContext.Transactions
                .FirstOrDefaultAsync(t => t.TransactionExternalId == transaction.TransactionExternalId);

            Assert.NotNull(updatedTransaction);
            Assert.Equal(Domain.Enums.TransactionStatus.Approved, updatedTransaction.Status);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
} 