using System;

namespace TransactionService.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a domain rule is violated in the Transaction context
    /// </summary>
    public class TransactionDomainException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionDomainException"/> class
        /// </summary>
        public TransactionDomainException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionDomainException"/> class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public TransactionDomainException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionDomainException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public TransactionDomainException(string message, Exception innerException) : base(message, innerException) { }
    }
} 