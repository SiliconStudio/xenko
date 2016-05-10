namespace SiliconStudio.Core.Transactions
{
    /// <summary>
    /// A enum listing the possible reasons for transactions to be discarded from an <see cref="ITransactionStack"/>.
    /// </summary>
    public enum DiscardReason
    {
        /// <summary>
        /// Transactions have been discarded because the stack is full.
        /// </summary>
        StackFull,
        /// <summary>
        /// Transactions have been discarded because the top of the stack has been purged.
        /// </summary>
        StackPurged,
    }
}
