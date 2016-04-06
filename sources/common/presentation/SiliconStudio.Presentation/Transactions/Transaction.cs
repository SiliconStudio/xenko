using System;
using System.Collections.Generic;
using System.Threading;

namespace SiliconStudio.Presentation.Transactions
{
    /// <summary>
    /// This class is the internal implementation of transaction.
    /// </summary>
    internal sealed class Transaction : Operation, ITransaction, IReadOnlyTransaction
    {
        private readonly List<Operation> operations = new List<Operation>();
        private readonly TransactionStack transactionStack;
        private SynchronizationContext synchronizationContext;
        private bool isCompleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="Transaction"/> class.
        /// </summary>
        /// <param name="transactionStack">The <see cref="TransactionStack"/> associated to this transaction.</param>
        public Transaction(TransactionStack transactionStack)
        {
            this.transactionStack = transactionStack;
            synchronizationContext = SynchronizationContext.Current;
        }

        /// <inheritdoc/>
        public bool IsEmpty => operations.Count == 0;

        /// <inheritdoc/>
        public IReadOnlyList<Operation> Operations => operations;

        /// <summary>
        /// Disposes the transaction by completing it and registering it to the transaction stack.
        /// </summary>
        /// <seealso cref="Complete"/>
        public void Dispose()
        {
            if (isCompleted)
                throw new TransactionException("This transaction has already been completed.");

            Complete();
        }

        /// <inheritdoc/>
        public IReadOnlyTransaction Complete()
        {
            if (isCompleted)
                throw new TransactionException("This transaction has already been completed.");

            if (synchronizationContext != SynchronizationContext.Current)
                throw new TransactionException("This transaction is being completed in a different synchronization context.");

            transactionStack.CompleteTransaction(this);

            // Don't keep reference to synchronization context after completion
            synchronizationContext = null;
            isCompleted = true;
            return this;
        }

        /// <summary>
        /// Pushes an operation in this transaction.
        /// </summary>
        /// <param name="operation">The operation to push.</param>
        /// <remarks>This method should be invoked by <seealso cref="TransactionStack"/> only.</remarks>
        internal void PushOperation(Operation operation)
        {
            operations.Add(operation);
        }

        /// <inheritdoc/>
        protected override void Rollback()
        {
            for (int i = operations.Count - 1; i >= 0; --i)
            {
                operations[i].Interface.Rollback();
            }
        }

        /// <inheritdoc/>
        protected override void Rollforward()
        {
            foreach (var operation in operations)
            {
                operation.Interface.Rollforward();
            }
        }

        /// <inheritdoc/>
        protected override void FreezeContent()
        {
            base.FreezeContent();
            foreach (var operation in operations)
            {
                operation.Interface.Freeze();
            }
        }
    }
}
