using System;
using System.Collections.Generic;

namespace SiliconStudio.Core.Transactions
{
    /// <summary>
    /// An completed interface that cannot be modified anymore, but can be rollbacked or rollforwarded.
    /// </summary>
    public interface IReadOnlyTransaction
    {
        /// <summary>
        /// Gets an unique identifier for the transaction.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the operations executed during the transaction.
        /// </summary>
        IReadOnlyList<Operation> Operations { get; }
    }
}
