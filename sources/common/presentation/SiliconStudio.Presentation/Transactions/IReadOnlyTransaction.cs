using System.Collections.Generic;

namespace SiliconStudio.Presentation.Transactions
{
    /// <summary>
    /// An completed interface that cannot be modified anymore, but can be rollbacked or rollforwarded.
    /// </summary>
    public interface IReadOnlyTransaction
    {
        /// <summary>
        /// Gets the operations executed during the transaction.
        /// </summary>
        IReadOnlyList<Operation> Operations { get; }
    }
}