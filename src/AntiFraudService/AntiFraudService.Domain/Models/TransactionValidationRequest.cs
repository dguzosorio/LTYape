using System;

namespace AntiFraudService.Domain.Models
{
    /// <summary>
    /// Representa una solicitud de validación de transacción recibida del servicio de transacciones
    /// </summary>
    public class TransactionValidationRequest
    {
        /// <summary>
        /// Identificador externo de la transacción
        /// </summary>
        public Guid TransactionExternalId { get; set; }
        
        /// <summary>
        /// Identificador de la cuenta de origen
        /// </summary>
        public Guid SourceAccountId { get; set; }
        
        /// <summary>
        /// Identificador de la cuenta de destino
        /// </summary>
        public Guid TargetAccountId { get; set; }
        
        /// <summary>
        /// Tipo de transferencia
        /// </summary>
        public int TransferTypeId { get; set; }
        
        /// <summary>
        /// Valor monetario de la transacción
        /// </summary>
        public decimal Value { get; set; }
        
        /// <summary>
        /// Fecha y hora de creación de la transacción
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
} 