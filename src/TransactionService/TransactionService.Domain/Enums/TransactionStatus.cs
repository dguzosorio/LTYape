namespace TransactionService.Domain.Enums
{
    /// <summary>
    /// Represents the possible states of a financial transaction
    /// </summary>
    public enum TransactionStatus
    {
        /// <summary>
        /// Transaction is created but not yet validated
        /// </summary>
        Pending = 1,
        
        /// <summary>
        /// Transaction has passed fraud validation and is approved
        /// </summary>
        Approved = 2,
        
        /// <summary>
        /// Transaction has failed fraud validation and is rejected
        /// </summary>
        Rejected = 3,

        /// <summary>
        /// Transaction has been successfully processed and completed
        /// </summary>
        Completed = 4
    }
} 