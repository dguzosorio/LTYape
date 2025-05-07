namespace AntiFraudService.Domain.Enums
{
    /// <summary>
    /// Represents the possible results of a fraud validation operation
    /// </summary>
    public enum ValidationResult
    {
        /// <summary>
        /// The transaction passes all fraud validation rules
        /// </summary>
        Approved = 1,
        
        /// <summary>
        /// The transaction fails one or more fraud validation rules
        /// </summary>
        Rejected = 2
    }
} 