using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SiliconStudio.ActionStack.Tests.Helpers
{
    class TransactionalActionStackTestContainer : ActionStackTestContainer
    {
        public TransactionalActionStackTestContainer(int capacity)
            : base(new TransactionalActionStack(capacity))
        {
            RegisterEvents();
        }

        public new TransactionalActionStack Stack => (TransactionalActionStack)base.Stack;

        public List<Tuple<object, EventArgs>> TransactionStarted = new List<Tuple<object, EventArgs>>();
        public List<Tuple<object, ActionItemsEventArgs<IActionItem>>> TransactionEnded = new List<Tuple<object, ActionItemsEventArgs<IActionItem>>>();
        public List<Tuple<object, ActionItemsEventArgs<IActionItem>>> TransactionCancelled = new List<Tuple<object, ActionItemsEventArgs<IActionItem>>>();
        public List<Tuple<object, ActionItemsEventArgs<IActionItem>>> TransactionDiscarded = new List<Tuple<object, ActionItemsEventArgs<IActionItem>>>();

        private void RegisterEvents()
        {
            Stack.TransactionStarted += (sender, e) => TransactionStarted.Add(Tuple.Create(sender, e));
            Stack.TransactionEnded += (sender, e) => TransactionEnded.Add(Tuple.Create(sender, e));
            Stack.TransactionCancelled += (sender, e) => TransactionCancelled.Add(Tuple.Create(sender, e));
            Stack.TransactionDiscarded += (sender, e) => TransactionDiscarded.Add(Tuple.Create(sender, e));
        }

        public void CheckTransactionCount(int begun, int ended, int cancelled, int discarded)
        {
            Assert.AreEqual(begun, TransactionStarted.Count);
            Assert.AreEqual(ended, TransactionEnded.Count);
            Assert.AreEqual(cancelled, TransactionCancelled.Count);
            Assert.AreEqual(discarded, TransactionDiscarded.Count);
        }
    }
}