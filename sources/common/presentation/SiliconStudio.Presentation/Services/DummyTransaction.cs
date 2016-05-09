using System;
using System.Collections.Generic;
using SiliconStudio.Core.Transactions;

namespace SiliconStudio.Presentation.Services
{
    /// <summary>
    /// A dummy transaction created when <see cref="IUndoRedoService.UndoRedoInProgress"/> is true and a new transaction is requested.
    /// Any operation pushed during this transaction will throw.
    /// </summary>
    internal class DummyTransaction : ITransaction, IReadOnlyTransaction
    {
        private bool isCompleted;

        public Guid Id { get; } = Guid.NewGuid();

        public IReadOnlyList<Operation> Operations { get; } = new Operation[0];

        public bool IsEmpty => true;

        public void Dispose()
        {
            if (isCompleted)
                throw new TransactionException("This transaction has already been completed.");

            Complete();
        }

        public void Continue()
        {
        }

        public void Complete()
        {
            if (isCompleted)
                throw new TransactionException("This transaction has already been completed.");

            isCompleted = true;
        }
    }
}
