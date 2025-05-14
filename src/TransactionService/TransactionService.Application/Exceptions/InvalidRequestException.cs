using System;

namespace TransactionService.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a request is invalid
    /// </summary>
    public class InvalidRequestException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidRequestException"/> class
        /// </summary>
        public InvalidRequestException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidRequestException"/> class with a specified error message
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception</param>
        public InvalidRequestException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidRequestException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public InvalidRequestException(string message, Exception innerException) : base(message, innerException) { }
    }
} 