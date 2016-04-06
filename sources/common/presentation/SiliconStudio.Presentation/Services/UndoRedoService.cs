using System;
using System.Collections.Generic;
using SiliconStudio.Presentation.Dirtiables;
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
        private readonly Dictionary<Guid, string> operationNames = new Dictionary<Guid, string>();
        private readonly DirtiableManager dirtiableManager;

        public UndoRedoService(int stackCapacity)
        {
            stack = TransactionStackFactory.Create(stackCapacity);
            dirtiableManager = new DirtiableManager(stack);
        }

        public int Capacity => stack.Capacity;

        public bool CanUndo => stack.CanRollback;

        public bool CanRedo => stack.CanRollforward;

        public event EventHandler<TransactionEventArgs> Done { add { stack.TransactionCompleted += value; } remove { stack.TransactionCompleted -= value; } }

        public event EventHandler<TransactionEventArgs> Undone { add { stack.TransactionRollbacked += value; } remove { stack.TransactionRollbacked -= value; } }

        public event EventHandler<TransactionEventArgs> Redone { add { stack.TransactionRollforwarded += value; } remove { stack.TransactionRollforwarded -= value; } }

        public event EventHandler<TransactionsDiscardedEventArgs> TransactionDiscarded { add { stack.TransactionDiscarded += value; } remove { stack.TransactionDiscarded -= value; } }

        public event EventHandler<EventArgs> Cleared { add { stack.Cleared += value; } remove { stack.Cleared -= value; } }

        public ITransaction CreateTransaction() => stack.CreateTransaction();

        public IEnumerable<IReadOnlyTransaction> RetrieveAllTransactions() => stack.RetrieveAllTransactions();

        public void PushOperation(Operation operation) => stack.PushOperation(operation);

        public void Undo() => stack.Rollback();

        public void Redo() => stack.Rollforward();

        public void NotifySave() => dirtiableManager.CreateSnapshot();

        public void SetName(Operation operation, string name)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            SetName(operation.Id, name);
        }

        public void SetName(ITransaction transaction, string name)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            SetName(transaction.Id, name);
        }

        public string GetName(Operation operation)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            return GetName(operation.Id) ?? operation.ToString();
        }

        public string GetName(ITransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            return GetName(transaction.Id) ?? transaction.ToString();
        }

        public string GetName(IReadOnlyTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            return GetName(transaction.Id) ?? transaction.ToString();
        }

        private void SetName(Guid id, string name)
        {
            if (name != null)
                operationNames[id] = name;
            else
                operationNames.Remove(id);
        }

        private string GetName(Guid id)
        {
            string name;
            operationNames.TryGetValue(id, out name);
            return name;
        }
    }
}
