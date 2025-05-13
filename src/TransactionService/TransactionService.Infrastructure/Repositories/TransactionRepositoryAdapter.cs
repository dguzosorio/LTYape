using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Ports;
using TransactionService.Infrastructure.Persistence;

namespace TransactionService.Infrastructure.Repositories
{
    /// <summary>
    /// Adaptador de repositorio de transacciones utilizando Entity Framework Core
    /// </summary>
    public class TransactionRepositoryAdapter : ITransactionRepositoryPort
    {
        private readonly TransactionDbContext _dbContext;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TransactionRepositoryAdapter"/>
        /// </summary>
        /// <param name="dbContext">El contexto de base de datos</param>
        public TransactionRepositoryAdapter(TransactionDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Recupera una transacción por su identificador externo y fecha de creación
        /// </summary>
        /// <param name="externalId">El ID externo de la transacción</param>
        /// <param name="createdAt">La fecha de creación de la transacción</param>
        /// <returns>La transacción si se encuentra, o null si no se encuentra</returns>
        public async Task<Transaction?> getByExternalIdAndDateAsync(Guid externalId, DateTime createdAt)
        {
            return await _dbContext.Transactions
                .FirstOrDefaultAsync(t => 
                    t.TransactionExternalId == externalId && 
                    t.CreatedAt.Date == createdAt.Date);
        }

        /// <summary>
        /// Añade una nueva transacción al repositorio
        /// </summary>
        /// <param name="transaction">La transacción a añadir</param>
        public async Task addAsync(Transaction transaction)
        {
            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Actualiza una transacción existente en el repositorio
        /// </summary>
        /// <param name="transaction">La transacción con datos actualizados</param>
        public async Task updateAsync(Transaction transaction)
        {
            _dbContext.Transactions.Update(transaction);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Calcula la suma total de transacciones para una cuenta en un día específico
        /// </summary>
        /// <param name="accountId">El identificador de la cuenta</param>
        /// <param name="date">La fecha para la cual calcular la suma</param>
        /// <returns>La suma de valores de transacción para la cuenta y fecha especificadas</returns>
        public async Task<decimal> getDailyTransactionSumForAccountAsync(Guid accountId, DateTime date)
        {
            return await _dbContext.Transactions
                .Where(t => t.SourceAccountId == accountId 
                    && t.CreatedAt.Date == date.Date)
                .SumAsync(t => t.Value);
        }
    }
} 