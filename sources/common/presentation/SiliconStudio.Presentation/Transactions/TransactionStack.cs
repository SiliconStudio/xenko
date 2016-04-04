using System;
using System.Collections.Generic;
using System.Threading;

namespace SiliconStudio.Presentation.Transactions
{
    interface IOperation
    {
        void Freeze();

        void Rollback();

        void Rollforward();
    }

    public class TransactionEventArgs : EventArgs
    {
        public TransactionEventArgs(IReadOnlyTransaction transaction)
        {
            Transaction = transaction;
        }

        public IReadOnlyTransaction Transaction { get; }
    }

    public enum DiscardReason
    {
        StackFull,
        StackPurged,
    }

    public class TransactionDiscardedEventArgs : EventArgs
    {
        public TransactionDiscardedEventArgs(IReadOnlyTransaction[] transactions, DiscardReason reason)
        {
            Transactions = transactions;
            Reason = reason;
        }

        public TransactionDiscardedEventArgs(IReadOnlyTransaction transaction, DiscardReason reason)
            : this(new[] { transaction }, reason)
        {
        }

        public IReadOnlyList<IReadOnlyTransaction> Transactions { get; }

        public DiscardReason Reason { get; set; }
    }

    public abstract class Operation : IOperation
    {
        private bool inProgress;

        internal bool IsFrozen { get; private set; }

        public void Freeze()
        {
            if (IsFrozen)
                throw new TransactionException("This operation is already frozen.");

            FreezeContent();
            IsFrozen = true;
        }

        void IOperation.Rollback()
        {
            if (IsFrozen)
                throw new TransactionException("A disposed operation cannot be rollbacked.");
            if (inProgress)
                throw new TransactionException("This operation is already in progress");

            inProgress = true;
            Rollback();
            inProgress = false;
        }

        void IOperation.Rollforward()
        {
            if (IsFrozen)
                throw new TransactionException("A disposed operation cannot be rollforwarded.");
            if (inProgress)
                throw new TransactionException("This operation is already in progress");

            inProgress = true;
            Rollforward();
            inProgress = false;
        }

        protected abstract void Rollback();

        protected abstract void Rollforward();

        protected virtual void FreezeContent()
        {
            // Do nothing by default
        }
    }

    public interface ITransaction : IDisposable
    {
        void Complete();
    }

    public interface IReadOnlyTransaction
    {
        IReadOnlyList<Operation> Operations { get; }
    }

    internal class Transaction : Operation, ITransaction, IReadOnlyTransaction
    {
        private readonly List<Operation> items = new List<Operation>();
        private readonly TransactionStack transactionStack;
        private SynchronizationContext synchronizationContext;
        private bool isCompleted;

        public Transaction(TransactionStack transactionStack)
        {
            this.transactionStack = transactionStack;
            synchronizationContext = SynchronizationContext.Current;
        }

        public bool IsEmpty => items.Count == 0;

        public IReadOnlyList<Operation> Operations => items;

        public void Dispose()
        {
            if (isCompleted)
                throw new TransactionException("This transaction has already been completed.");

            Complete();
        }

        protected override void Rollback()
        {
            for (int i = items.Count - 1; i >= 0; --i)
            {
                ((IOperation)items[i]).Rollback();
            }
        }

        protected override void Rollforward()
        {
            foreach (var operation in items)
            {
                ((IOperation)operation).Rollforward();
            }
        }

        protected override void FreezeContent()
        {
            base.FreezeContent();
            foreach (var operation in items)
            {
                operation.Freeze();
            }
        }

        public void Complete()
        {
            if (isCompleted)
                throw new TransactionException("This transaction has already been completed.");

            if (synchronizationContext != SynchronizationContext.Current)
                throw new TransactionException("This transaction is being completed in a different synchronization context.");

            transactionStack.CompleteTransaction(this);

            // Don't keep reference to synchronization context after completion
            synchronizationContext = null;
            isCompleted = true;
        }

        public void PushOperation(Operation operation)
        {
            items.Add(operation);
        }
    }

    public interface ITransactionStack
    {
        //IReadOnlyList<IReadOnlyOperation> History { get; }

        int Capacity { get; }

        bool IsEmpty { get; }

        bool IsFull { get; }

        bool CanRollback { get; }

        bool CanRollforward { get; }

        event EventHandler<TransactionEventArgs> TransactionCompleted;

        event EventHandler<TransactionEventArgs> TransactionRollbacked;

        event EventHandler<TransactionEventArgs> TransactionRollforwarded;

        event EventHandler<TransactionDiscardedEventArgs> TransactionDiscarded;

        event EventHandler<EventArgs> Cleared;

        void Clear();

        ITransaction CreateTransaction();

        IAsyncTransaction CreateAsyncTransaction();

        void PushOperation(Operation operation);

        void Rollback();

        void Rollforward();
    }

    public interface IAsyncTransaction
    {
    }

    public class TransactionException : Exception
    {
        public TransactionException(string message)
            : base(message)
        {

        }
    }

    public static class TransactionStackFactory
    {
        public static ITransactionStack Create(int capacity)
        {
            return new TransactionStack(capacity);
        }
    }

    internal class TransactionStack : ITransactionStack
    {        
        private readonly List<Transaction> transactions = new List<Transaction>();
        private readonly List<Transaction> frozenTransactions = new List<Transaction>();
        private readonly Stack<Transaction> transactionsInProgress = new Stack<Transaction>();
        private readonly object lockObject = new object();
        private int currentPosition;

        public TransactionStack(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            Capacity = capacity;
        }

        public IReadOnlyList<IReadOnlyTransaction> Transactions => transactions;

        public IReadOnlyList<IReadOnlyTransaction> FrozenTransactions => frozenTransactions;

        public int Capacity { get; }

        public bool IsEmpty => Transactions.Count == 0;

        public bool IsFull => Transactions.Count == Capacity;

        public bool CanRollback => currentPosition > 0;

        public bool CanRollforward => currentPosition < transactions.Count;

        public bool InProgress { get; private set; }

        public event EventHandler<TransactionEventArgs> TransactionCompleted;

        public event EventHandler<TransactionEventArgs> TransactionRollbacked;

        public event EventHandler<TransactionEventArgs> TransactionRollforwarded;

        public event EventHandler<TransactionDiscardedEventArgs> TransactionDiscarded;

        public event EventHandler<EventArgs> Cleared;

        public void Clear()
        {
            lock (lockObject)
            {
                if (InProgress)
                    throw new TransactionException("Unable to clear. A rollback or rollforward operation is in progress.");

                foreach (var transaction in transactions)
                {
                    transaction.Freeze();
                }
                transactions.Clear();
                currentPosition = 0;
                Cleared?.Invoke(this, EventArgs.Empty);
            }
        }

        public ITransaction CreateTransaction()
        {
            lock (lockObject)
            {
                if (InProgress)
                    throw new TransactionException("Unable to create a transaction. A rollback or rollforward operation is in progress.");

                var transaction = new Transaction(this);
                transactionsInProgress.Push(transaction);
                return transaction;
            }
        }

        public IAsyncTransaction CreateAsyncTransaction()
        {
            throw new NotImplementedException();
        }

        public void PushOperation(Operation operation)
        {
            lock (lockObject)
            {
                if (transactionsInProgress.Count == 0)
                    throw new TransactionException("There is not transaction in progress in the transaction stack.");

                var transaction = transactionsInProgress.Peek();
                transaction.PushOperation(operation);
            }
        }

        public void CompleteTransaction(Transaction transaction)
        {
            lock (lockObject)
            {
                if (transactionsInProgress.Count == 0)
                    throw new TransactionException("There is not transaction in progress in the transaction stack.");

                if (transaction != transactionsInProgress.Pop())
                    throw new TransactionException("The transaction being completed is not that last created transaction.");

                // Ignore the transaction if it is empty
                if (!transaction.IsEmpty)
                {
                    // Remove transactions that will be overwritten by this one
                    if (currentPosition < transactions.Count)
                    {
                        PurgeFromIndex(currentPosition, true);
                    }

                    if (currentPosition == Capacity)
                    {
                        // If the stack has a capacity of 0, immediately freeze the new transaction.
                        var oldestTransaction = Capacity > 0 ? transactions[0] : transaction;
                        oldestTransaction.Freeze();
                        frozenTransactions.Add(oldestTransaction);

                        for (var i = 1; i < transactions.Count; ++i)
                        {
                            transactions[i - 1] = transactions[i];
                        }
                        if (Capacity > 0)
                        {
                            --currentPosition;
                            PurgeFromIndex(currentPosition, false);
                        }
                        TransactionDiscarded?.Invoke(this, new TransactionDiscardedEventArgs(oldestTransaction, DiscardReason.StackFull));
                    }
                    if (Capacity > 0)
                    {
                        transactions.Insert(currentPosition, transaction);
                        ++currentPosition;
                    }
                }

                TransactionCompleted?.Invoke(this, new TransactionEventArgs(transaction));
            }
        }

        private void PurgeFromIndex(int index, bool raiseEvent)
        {
            if (index < 0 || index > transactions.Count) throw new ArgumentOutOfRangeException(nameof(index));

            if (transactions.Count - index > 0)
            {
                var discardedTransaction = raiseEvent ? new IReadOnlyTransaction[transactions.Count - index] : null;
                if (raiseEvent)
                {
                    for (var i = index; i < transactions.Count - index; ++i)
                    {
                        discardedTransaction[i - index] = transactions[i];
                    }
                }
                transactions.RemoveRange(index, transactions.Count - index);
                if (raiseEvent)
                {
                    TransactionDiscarded?.Invoke(this, new TransactionDiscardedEventArgs(discardedTransaction, DiscardReason.StackPurged));
                }
            }
        }

        public void Rollback()
        {
            lock (lockObject)
            {
                if (!CanRollback)
                    throw new TransactionException("Unable to rollback. This method cannot be invoked when CanRollback is false.");
                if (InProgress)
                    throw new TransactionException("Unable to rollback. A rollback or rollforward operation is already in progress.");

                var lastTransaction = transactions[--currentPosition];
                InProgress = true;
                ((IOperation)lastTransaction).Rollback();
                InProgress = false;
                TransactionRollbacked?.Invoke(this, new TransactionEventArgs(lastTransaction));
            }
        }

        public void Rollforward()
        {
            lock (lockObject)
            {
                if (!CanRollforward)
                    throw new TransactionException("Unable to rollforward. This method cannot be invoked when CanRollforward is false.");
                if (InProgress)
                    throw new TransactionException("Unable to rollforward. A rollback or rollforward operation is already in progress.");

                var lastTransaction = transactions[currentPosition++];
                InProgress = true;
                ((IOperation)lastTransaction).Rollforward();
                InProgress = false;
                TransactionRollforwarded?.Invoke(this, new TransactionEventArgs(lastTransaction));
            }
        }

        //public void CompleteOperation(Operation operation)
        //{
        //    // TODO
        //    OperationInProgress = false;
        //    CurrentOperation = null;
        //}
    }
}
