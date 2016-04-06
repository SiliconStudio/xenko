using System;
using SiliconStudio.Presentation.Transactions;

namespace SiliconStudio.Presentation.Services
{
    /// <summary>
    /// A class representing a transaction for the undo/redo stack.
    /// </summary>
    public class UndoRedoTransaction : IUndoRedoTransaction
    {
        private ITransaction transaction;
        private IReadOnlyTransaction completedTransaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="UndoRedoTransaction"/> class.
        /// </summary>
        /// <param name="stack"></param>
        public UndoRedoTransaction(ITransactionStack stack)
        {
            transaction = stack.CreateTransaction();
        }

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        public bool IsCompleted => completedTransaction != null;

        /// <summary>
        /// Completes the transaction.
        /// </summary>
        public void Dispose()
        {
            completedTransaction = transaction.Complete();
            transaction = null;
        }
    }

    public class UndoRedoService : IUndoRedoService
    {
        private readonly ITransactionStack stack;

        public UndoRedoService(int stackCapacity)
        {
            stack = TransactionStackFactory.Create(stackCapacity);
        }

        public int Capacity => stack.Capacity;

        public bool CanUndo => stack.CanRollback;

        public bool CanRedo => stack.CanRollforward;

        public event EventHandler<TransactionEventArgs> Done { add { stack.TransactionCompleted += value; } remove { stack.TransactionCompleted -= value; } }

        public event EventHandler<TransactionEventArgs> Undone { add { stack.TransactionRollbacked += value; } remove { stack.TransactionRollbacked -= value; } }

        public event EventHandler<TransactionEventArgs> Redone { add { stack.TransactionRollforwarded += value; } remove { stack.TransactionRollforwarded -= value; } }

        public event EventHandler<TransactionsDiscardedEventArgs> TransactionDiscarded { add { stack.TransactionDiscarded += value; } remove { stack.TransactionDiscarded -= value; } }

        public event EventHandler<EventArgs> Cleared { add { stack.Cleared += value; } remove { stack.Cleared -= value; } }

        public UndoRedoTransaction CreateTransaction() => new UndoRedoTransaction(stack);

        public void PushOperation(Operation operation) => stack.PushOperation(operation);

        public void Undo() => stack.Rollback();

        public void Redo() => stack.Rollforward();
    }
}
