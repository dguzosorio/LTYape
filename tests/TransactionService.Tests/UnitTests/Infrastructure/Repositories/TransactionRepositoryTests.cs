using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure.Persistence;
using TransactionService.Infrastructure.Repositories;
using Xunit;

namespace TransactionService.Tests.UnitTests.Infrastructure.Repositories
{
    public class TransactionRepositoryAdapterTests : IDisposable
    {
        private readonly TransactionDbContext _dbContext;
        private readonly TransactionRepositoryAdapter _repository;

        public TransactionRepositoryAdapterTests()
        {
            var options = new DbContextOptionsBuilder<TransactionDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new TransactionDbContext(options);
            _repository = new TransactionRepositoryAdapter(_dbContext);
        }

        [Fact]
        public async Task getByExternalIdAndDateAsync_WhenTransactionExists_ReturnsTransaction()
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
            var result = await _repository.getByExternalIdAndDateAsync(
                transaction.TransactionExternalId,
                transaction.CreatedAt);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transaction.TransactionExternalId, result.TransactionExternalId);
            Assert.Equal(transaction.CreatedAt.Date, result.CreatedAt.Date);
        }

        [Fact]
        public async Task getByExternalIdAndDateAsync_WhenTransactionDoesNotExist_ReturnsNull()
        {
            // Arrange
            var externalId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;

            // Act
            var result = await _repository.getByExternalIdAndDateAsync(externalId, createdAt);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task getByExternalIdAndDateAsync_WhenTransactionExistsButDateDoesNotMatch_ReturnsNull()
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
            var result = await _repository.getByExternalIdAndDateAsync(
                transaction.TransactionExternalId,
                transaction.CreatedAt.AddDays(1));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task addAsync_AddsTransactionToDatabase()
        {
            // Arrange
            var transaction = new Transaction(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                100
            );

            // Act
            await _repository.addAsync(transaction);

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
        public async Task updateAsync_UpdatesTransactionInDatabase()
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
            await _repository.updateAsync(transaction);

            // Assert
            var updatedTransaction = await _dbContext.Transactions
                .FirstOrDefaultAsync(t => t.TransactionExternalId == transaction.TransactionExternalId);

            Assert.NotNull(updatedTransaction);
            Assert.Equal(Domain.Enums.TransactionStatus.Approved, updatedTransaction.Status);
        }

        [Fact]
        public async Task getDailyTransactionSumForAccountAsync_ReturnsCorrectTotal()
        {
            // Arrange
            var sourceAccountId = Guid.NewGuid();
            var date = DateTime.UtcNow.Date;
            
            // Add transactions for today
            var transaction1 = new Transaction(
                sourceAccountId,
                Guid.NewGuid(),
                1,
                100
            );
            
            var transaction2 = new Transaction(
                sourceAccountId,
                Guid.NewGuid(),
                1,
                200
            );
            
            // Add transaction for a different date (should not be counted)
            var transaction3 = new Transaction(
                sourceAccountId,
                Guid.NewGuid(),
                1,
                300
            );
            transaction3.CreatedAt = date.AddDays(-1);
            
            // Add transaction for a different account (should not be counted)
            var transaction4 = new Transaction(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                400
            );
            
            await _dbContext.Transactions.AddRangeAsync(transaction1, transaction2, transaction3, transaction4);
            await _dbContext.SaveChangesAsync();
            
            // Act
            var result = await _repository.getDailyTransactionSumForAccountAsync(sourceAccountId, date);
            
            // Assert
            Assert.Equal(300, result); // 100 + 200
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
} 