using System;
using System.Threading;

namespace SiliconStudio.Core.Transactions
{
    /// <summary>
    /// An interface representing a transaction currently in progress. The transaction must be
    /// completed in the same <see cref="SynchronizationContext"/> it was created.
    /// </summary>
    public interface ITransaction : IDisposable
    {
        /// <summary>
        /// Gets an unique identifier for the transaction.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets whether this transaction is empty.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Continues the transaction when the current <see cref="SynchronizationContext"/> has changed, allowing to push additional operations or complete it.
        /// </summary>
        void Continue();

        /// <summary>
        /// Completes the transaction by closing it and adding it to the transaction stack.
        /// </summary>
        /// <remarks>This method is invoked by the <see cref="IDisposable.Dispose"/> method.</remarks>
        void Complete();
    }
}
