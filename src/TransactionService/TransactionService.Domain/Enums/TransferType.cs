namespace TransactionService.Domain.Enums
{
    /// <summary>
    /// Represents the types of financial transfers supported by the system
    /// </summary>
    public enum TransferType
    {
        /// <summary>
        /// Standard bank transfer between accounts
        /// </summary>
        Standard = 1,
        
        /// <summary>
        /// ATM cash withdrawal
        /// </summary>
        ATMWithdrawal = 2,
        
        /// <summary>
        /// International transfer to foreign accounts
        /// </summary>
        International = 3
    }
} 