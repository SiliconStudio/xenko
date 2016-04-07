using System;
using System.Collections.Generic;

namespace SiliconStudio.Core.Transactions
{
    /// <summary>
    /// Internal implementation of the <see cref="ITransactionStack"/> interface.
    /// </summary>
    internal class TransactionStack : ITransactionStack
    {
        private readonly List<Transaction> transactions = new List<Transaction>();
        private readonly Stack<Transaction> transactionsInProgress = new Stack<Transaction>();
        private readonly object lockObject = new object();
        private int currentPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionStack"/> class.
        /// </summary>
        /// <param name="capacity">The capacity of the stack.</param>
        public TransactionStack(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            Capacity = capacity;
        }

        /// <summary>
        /// Gets the collection of transactions currently on the stack.
        /// </summary>
        public IReadOnlyList<IReadOnlyTransaction> Transactions => transactions;

        /// <summary>
        /// Gets whether there is a transaction rollback or rollforward currently in progress.
        /// </summary>
        public bool RollInProgress { get; private set; }

        /// <inheritdoc/>
        public int Capacity { get; private set; }

        /// <inheritdoc/>
        public bool IsEmpty => Transactions.Count == 0;

        /// <inheritdoc/>
        public bool IsFull => Transactions.Count == Capacity;

        /// <inheritdoc/>
        public bool CanRollback => currentPosition > 0;

        /// <inheritdoc/>
        public bool CanRollforward => currentPosition < transactions.Count;

        /// <inheritdoc/>
        public event EventHandler<TransactionEventArgs> TransactionCompleted;

        /// <inheritdoc/>
        public event EventHandler<TransactionEventArgs> TransactionRollbacked;

        /// <inheritdoc/>
        public event EventHandler<TransactionEventArgs> TransactionRollforwarded;

        /// <inheritdoc/>
        public event EventHandler<TransactionsDiscardedEventArgs> TransactionDiscarded;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> Cleared;

        /// <inheritdoc/>
        public ITransaction CreateTransaction()
        {
            lock (lockObject)
            {
                if (RollInProgress)
                    throw new TransactionException("Unable to create a transaction. A rollback or rollforward operation is in progress.");

                var transaction = new Transaction(this);
                transactionsInProgress.Push(transaction);
                return transaction;
            }
        }

        /// <inheritdoc/>
        public IAsyncTransaction CreateAsyncTransaction()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void Clear()
        {
            lock (lockObject)
            {
                if (RollInProgress)
                    throw new TransactionException("Unable to clear. A rollback or rollforward operation is in progress.");

                foreach (var transaction in transactions)
                {
                    transaction.Interface.Freeze();
                }
                transactions.Clear();
                currentPosition = 0;
                Cleared?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IReadOnlyTransaction> RetrieveAllTransactions()
        {
            return transactions;
        }

        /// <inheritdoc/>
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
                        PurgeFromIndex(currentPosition);
                    }

                    if (currentPosition == Capacity)
                    {
                        // If the stack has a capacity of 0, immediately freeze the new transaction.
                        var oldestTransaction = Capacity > 0 ? transactions[0] : transaction;
                        oldestTransaction.Interface.Freeze();

                        for (var i = 1; i < transactions.Count; ++i)
                        {
                            transactions[i - 1] = transactions[i];
                        }
                        if (Capacity > 0)
                        {
                            transactions[--currentPosition] = null;
                        }
                        TransactionDiscarded?.Invoke(this, new TransactionsDiscardedEventArgs(oldestTransaction, DiscardReason.StackFull));
                    }
                    if (Capacity > 0)
                    {
                        if (currentPosition == transactions.Count)
                        {
                            transactions.Add(transaction);
                        }
                        else
                        {
                            transactions[currentPosition] = transaction;
                        }
                        ++currentPosition;
                    }

                    TransactionCompleted?.Invoke(this, new TransactionEventArgs(transaction));
                }
            }
        }

        /// <inheritdoc/>
        public void Rollback()
        {
            lock (lockObject)
            {
                if (!CanRollback)
                    throw new TransactionException("Unable to rollback. This method cannot be invoked when CanRollback is false.");
                if (RollInProgress)
                    throw new TransactionException("Unable to rollback. A rollback or rollforward operation is already in progress.");

                var lastTransaction = transactions[--currentPosition];
                RollInProgress = true;
                lastTransaction.Interface.Rollback();
                RollInProgress = false;
                TransactionRollbacked?.Invoke(this, new TransactionEventArgs(lastTransaction));
            }
        }

        /// <inheritdoc/>
        public void Rollforward()
        {
            lock (lockObject)
            {
                if (!CanRollforward)
                    throw new TransactionException("Unable to rollforward. This method cannot be invoked when CanRollforward is false.");
                if (RollInProgress)
                    throw new TransactionException("Unable to rollforward. A rollback or rollforward operation is already in progress.");

                var lastTransaction = transactions[currentPosition++];
                RollInProgress = true;
                lastTransaction.Interface.Rollforward();
                RollInProgress = false;
                TransactionRollforwarded?.Invoke(this, new TransactionEventArgs(lastTransaction));
            }
        }

        public void Resize(int newCapacity)
        {
            if (newCapacity < Capacity)
            {
                // TODO: this is minor but we should support that (potential discard, properly trigger events, etc.)
                throw new NotSupportedException("Resizing transaction stack to a smaller size is not supported yet.");
            }
            lock (lockObject)
            {
                Capacity = newCapacity;
            }
        }

        /// <summary>
        /// Purges the stack from the given index (included) to the top of the stack.
        /// </summary>
        /// <param name="index">The index from which to purge the stack.</param>
        private void PurgeFromIndex(int index)
        {
            if (index < 0 || index > transactions.Count) throw new ArgumentOutOfRangeException(nameof(index));

            if (transactions.Count - index > 0)
            {
                var discardedTransactions = new IReadOnlyTransaction[transactions.Count - index];
                for (var i = index; i < transactions.Count; ++i)
                {
                    transactions[i].Interface.Freeze();
                    discardedTransactions[i - index] = transactions[i];
                }
                transactions.RemoveRange(index, transactions.Count - index);
                TransactionDiscarded?.Invoke(this, new TransactionsDiscardedEventArgs(discardedTransactions, DiscardReason.StackPurged));
            }
        }
    }
}
