namespace SiliconStudio.Core.Transactions
{
    /// <summary>
    /// An interface representing an asynchronous transaction. An asynchronous transaction is a transaction that can be completed asynchronously. It
    /// provides additional safety such as preventing another asynchronous transaction to be created when there is one already in progress.
    /// </summary>
    public interface IAsyncTransaction
    {
    }
}
