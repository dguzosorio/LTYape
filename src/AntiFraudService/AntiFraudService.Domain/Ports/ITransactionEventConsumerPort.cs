using System;
using System.Threading.Tasks;
using AntiFraudService.Domain.Models;

namespace AntiFraudService.Domain.Ports
{
    /// <summary>
    /// Puerto para consumir eventos de transacciones
    /// </summary>
    public interface ITransactionEventConsumerPort
    {
        /// <summary>
        /// Suscribe una función de callback para procesar eventos de validación de transacciones
        /// </summary>
        /// <param name="handler">La función que procesará los eventos de transacciones</param>
        /// <returns>Una tarea que representa la operación asíncrona</returns>
        Task subscribeToTransactionValidationRequestsAsync(Func<TransactionValidationRequest, Task> handler);
    }
} 