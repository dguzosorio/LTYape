using System.Threading.Tasks;
using AntiFraudService.Domain.Models;

namespace AntiFraudService.Domain.Ports
{
    /// <summary>
    /// Puerto para la comunicación de eventos con el TransactionService
    /// </summary>
    public interface ITransactionEventPort
    {
        /// <summary>
        /// Envía una respuesta de validación al servicio de transacciones
        /// </summary>
        /// <param name="response">La respuesta de validación a enviar</param>
        /// <returns>Una tarea que representa la operación asíncrona</returns>
        Task sendValidationResponseAsync(ValidationResponse response);
    }
} 