namespace SiliconStudio.Core.Transactions
{
    /// <summary>
    /// A static factory to create <see cref="ITransactionStack"/> instances.
    /// </summary>
    public static class TransactionStackFactory
    {
        public static ITransactionStack Create(int capacity)
        {
            return new TransactionStack(capacity);
        }
    }
}
