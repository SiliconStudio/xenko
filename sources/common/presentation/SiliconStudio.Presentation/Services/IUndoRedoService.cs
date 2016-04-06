using System;
using System.Collections.Generic;
using SiliconStudio.Presentation.Transactions;

namespace SiliconStudio.Presentation.Services
{
    /// <summary>
    /// Interface for transactions of the <see cref="IUndoRedoService"/>.
    /// </summary>
    public interface IUndoRedoTransaction : IDisposable
    {
        /// <summary>
        /// Gets or sets the name of this transaction.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets whether this transaction is completed.
        /// </summary>
        bool IsCompleted { get; }
    }

    public interface IUndoRedoService
    {
        int Capacity { get; }

        bool CanUndo { get; }

        bool CanRedo { get; }

        /// <summary>
        /// Retrieves the collection of transactions registered to this service.
        /// </summary>
        /// <returns>A collection of transactions registered into this service.</returns>
        IEnumerable<IReadOnlyTransaction> RetrieveAllTransactions();

        event EventHandler<TransactionEventArgs> Done;

        event EventHandler<TransactionEventArgs> Undone;

        event EventHandler<TransactionEventArgs> Redone;

        event EventHandler<TransactionsDiscardedEventArgs> TransactionDiscarded;

        event EventHandler<EventArgs> Cleared;

        ITransaction CreateTransaction();

        void SetName(Operation operation, string name);

        void SetName(ITransaction transaction, string name);

        string GetName(Operation operation);

        string GetName(ITransaction transaction);

        string GetName(IReadOnlyTransaction transaction);

        void PushOperation(Operation operation);

        void Undo();

        void Redo();

        void NotifySave();
    }
}
