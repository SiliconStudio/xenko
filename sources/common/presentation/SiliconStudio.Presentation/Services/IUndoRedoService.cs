using System;
using System.Collections.Generic;
using SiliconStudio.Core.Transactions;

namespace SiliconStudio.Presentation.Services
{
    public interface IUndoRedoService
    {
        int Capacity { get; }

        bool CanUndo { get; }

        bool CanRedo { get; }

        bool UndoRedoInProgress { get; }

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

        void Resize(int newCapacity);
    }
}
