using System;

namespace AntiFraudService.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a domain rule is violated in the AntiFraud context
    /// </summary>
    public class AntiFraudDomainException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AntiFraudDomainException"/> class
        /// </summary>
        public AntiFraudDomainException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AntiFraudDomainException"/> class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public AntiFraudDomainException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AntiFraudDomainException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public AntiFraudDomainException(string message, Exception innerException) : base(message, innerException) { }
    }
} 