using System;
using SiliconStudio.Presentation.Transactions;

namespace SiliconStudio.Presentation.Services
{
    public class UndoRedoService
    {
        private readonly ITransactionStack stack;

        public UndoRedoService(int stackCapacity)
        {
            stack = TransactionStackFactory.Create(stackCapacity);
        }

        public int Capacity => stack.Capacity;

        public bool CanUndo => stack.CanRollback;

        public bool CanRedo => stack.CanRollforward;

        public event EventHandler<TransactionEventArgs> TransactionCompleted { add { stack.TransactionCompleted += value; } remove { stack.TransactionCompleted -= value; } }

        public event EventHandler<TransactionEventArgs> TransactionRollbacked { add { stack.TransactionRollbacked += value; } remove { stack.TransactionRollbacked -= value; } }

        public event EventHandler<TransactionEventArgs> TransactionRollforwarded { add { stack.TransactionRollforwarded += value; } remove { stack.TransactionRollforwarded -= value; } }

        public event EventHandler<TransactionsDiscardedEventArgs> TransactionDiscarded { add { stack.TransactionDiscarded += value; } remove { stack.TransactionDiscarded -= value; } }

        public event EventHandler<EventArgs> Cleared { add { stack.Cleared += value; } remove { stack.Cleared -= value; } }

        public ITransaction CreateTransaction() => stack.CreateTransaction();

        public void PushOperation(Operation operation) => stack.PushOperation(operation);

        public void Undo()
        {
            stack.Rollback();
        }

        public void Redo()
        {
            stack.Rollforward();
        }
    }
}