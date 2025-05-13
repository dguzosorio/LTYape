using System.Threading.Tasks;
using TransactionService.Domain.Entities;

namespace TransactionService.Domain.Ports
{
    /// <summary>
    /// Puerto para la comunicación de eventos antifraude
    /// </summary>
    public interface IAntiFraudEventPort
    {
        /// <summary>
        /// Envía una transacción para validación antifraude
        /// </summary>
        /// <param name="transaction">La transacción a validar</param>
        /// <returns>Una tarea que representa la operación asíncrona</returns>
        Task sendTransactionForValidationAsync(Transaction transaction);
    }
} 