namespace AntiFraudService.Domain.Enums
{
    /// <summary>
    /// Specifies the reason why a transaction was rejected during fraud validation
    /// </summary>
    public enum RejectionReason
    {
        /// <summary>
        /// No rejection - transaction is approved
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Transaction amount exceeds the maximum allowed value
        /// </summary>
        ExceedsMaximumAmount = 1,
        
        /// <summary>
        /// Transaction would cause the daily accumulated amount to exceed the maximum allowed
        /// </summary>
        ExceedsDailyLimit = 2,
        
        /// <summary>
        /// Transaction was rejected for other or multiple reasons
        /// </summary>
        Other = 99
    }
} 