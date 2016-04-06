using System;
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

        event EventHandler<TransactionEventArgs> Done;

        event EventHandler<TransactionEventArgs> Undone;

        event EventHandler<TransactionEventArgs> Redone;

        event EventHandler<TransactionsDiscardedEventArgs> TransactionDiscarded;

        event EventHandler<EventArgs> Cleared;

        UndoRedoTransaction CreateTransaction();

        void PushOperation(Operation operation);

        void Undo();

        void Redo();
    }
}
