using System;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.ActionStack.Tests.Helpers;

namespace SiliconStudio.ActionStack.Tests
{
    [TestFixture]
    class TestTransactionalActionStack
    {
        [Test]
        public void TestConstruction()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            Assert.AreEqual(false, stack.Stack.TransactionInProgress);
            Assert.AreEqual(0, stack.Stack.ActionItems.Count());
        }

        [Test]
        public void TestExceptions()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            Assert.Throws<InvalidOperationException>(() => stack.Stack.EndTransaction(""));
            Assert.Throws<InvalidOperationException>(() => stack.Stack.DiscardTransaction());
            Assert.Throws<InvalidOperationException>(() => stack.Stack.CancelTransaction());
            stack.Stack.BeginTransaction();
            Assert.DoesNotThrow(() => stack.Stack.EndTransaction(""));
            stack.Stack.BeginTransaction();
            Assert.DoesNotThrow(() => stack.Stack.DiscardTransaction());
            stack.Stack.BeginTransaction();
            Assert.DoesNotThrow(() => stack.Stack.CancelTransaction());
        }

        [Test]
        public void TestBeginEnd()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            stack.Stack.BeginTransaction();
            stack.Stack.Add(new SimpleActionItem());
            stack.Stack.Add(new SimpleActionItem());
            stack.Stack.Add(new SimpleActionItem());
            var current = stack.Stack.GetCurrentTransactions();
            Assert.AreEqual(3, current.Count);
            stack.Stack.EndTransaction("Test");
            Assert.Throws<InvalidOperationException>(() => stack.Stack.GetCurrentTransactions());
            stack.CheckRaiseCount(1, 0, 0, 0, 0);
            stack.CheckTransactionCount(1, 1, 0, 0);
        }

        [Test]
        public void TestUsingBeginEnd()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            using (stack.Stack.BeginEndTransaction("Test"))
            {
                stack.Stack.Add(new SimpleActionItem());
                stack.Stack.Add(new SimpleActionItem());
                stack.Stack.Add(new SimpleActionItem());
                var current = stack.Stack.GetCurrentTransactions();
                Assert.AreEqual(3, current.Count);
            }
            Assert.Throws<InvalidOperationException>(() => stack.Stack.GetCurrentTransactions());
            stack.CheckRaiseCount(1, 0, 0, 0, 0);
            stack.CheckTransactionCount(1, 1, 0, 0);
        }

        [Test]
        public void TestEmptyTransaction()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            stack.Stack.BeginTransaction();
            var current = stack.Stack.GetCurrentTransactions();
            Assert.AreEqual(0, current.Count);
            stack.Stack.EndTransaction("Test");
            Assert.Throws<InvalidOperationException>(() => stack.Stack.GetCurrentTransactions());
            stack.CheckRaiseCount(0, 0, 0, 0, 0);
            stack.CheckTransactionCount(1, 0, 0, 1);
        }

        [Test]
        public void TestSingleItemTransaction()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            stack.Stack.BeginTransaction();
            var current = stack.Stack.GetCurrentTransactions();
            Assert.AreEqual(0, current.Count);
            stack.Stack.Add(new SimpleActionItem());
            stack.Stack.EndTransaction("Test");
            Assert.Throws<InvalidOperationException>(() => stack.Stack.GetCurrentTransactions());
            // The transaction should be removed and replaced by the actual item
            var items = stack.Stack.ActionItems.ToList();
            Assert.AreEqual(1, items.Count);
            Assert.IsInstanceOf<SimpleActionItem>(items[0]);
            stack.CheckRaiseCount(1, 0, 0, 0, 0);
            stack.CheckTransactionCount(1, 1, 0, 0);
        }

        [Test]
        public void TestNestedSingleItemTransaction()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            stack.Stack.BeginTransaction();
            stack.Stack.BeginTransaction();
            var current = stack.Stack.GetCurrentTransactions();
            Assert.AreEqual(0, current.Count);
            stack.Stack.Add(new SimpleActionItem());
            stack.Stack.EndTransaction("Test");
            stack.Stack.EndTransaction("Test");
            Assert.Throws<InvalidOperationException>(() => stack.Stack.GetCurrentTransactions());
            // The transaction should be removed and replaced by the actual item
            var items = stack.Stack.ActionItems.ToList();
            Assert.AreEqual(1, items.Count);
            Assert.IsInstanceOf<SimpleActionItem>(items[0]);
            stack.CheckRaiseCount(1, 0, 0, 0, 0);
            stack.CheckTransactionCount(2, 2, 0, 0);
        }

        [Test]
        public void TestBeginCancel()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            stack.Stack.BeginTransaction();
            int orderCheck = 0;
            var action1 = new SimpleActionItem(); action1.OnUndo += () => { Assert.AreEqual(2, orderCheck); orderCheck++; };
            var action2 = new SimpleActionItem(); action2.OnUndo += () => { Assert.AreEqual(1, orderCheck); orderCheck++; };
            var action3 = new SimpleActionItem(); action3.OnUndo += () => { Assert.AreEqual(0, orderCheck); orderCheck++; };
            stack.Stack.Add(action1);
            stack.Stack.Add(action2);
            stack.Stack.Add(action3);
            var current = stack.Stack.GetCurrentTransactions();
            Assert.AreEqual(3, current.Count);
            stack.Stack.CancelTransaction();
            Assert.AreEqual(3, orderCheck);
            Assert.Throws<InvalidOperationException>(() => stack.Stack.GetCurrentTransactions());
            stack.CheckRaiseCount(0, 0, 0, 0, 0);
            stack.CheckTransactionCount(1, 0, 1, 0);
        }

        [Test]
        public void TestUsingBeginCancel()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            int orderCheck = 0;
            using (stack.Stack.BeginCancelTransaction())
            {
                var action1 = new SimpleActionItem(); action1.OnUndo += () => { Assert.AreEqual(2, orderCheck); orderCheck++; };
                var action2 = new SimpleActionItem(); action2.OnUndo += () => { Assert.AreEqual(1, orderCheck); orderCheck++; };
                var action3 = new SimpleActionItem(); action3.OnUndo += () => { Assert.AreEqual(0, orderCheck); orderCheck++; };
                stack.Stack.Add(action1);
                stack.Stack.Add(action2);
                stack.Stack.Add(action3);
                var current = stack.Stack.GetCurrentTransactions();
                Assert.AreEqual(3, current.Count);
            }
            Assert.AreEqual(3, orderCheck);
            Assert.Throws<InvalidOperationException>(() => stack.Stack.GetCurrentTransactions());
            stack.CheckRaiseCount(0, 0, 0, 0, 0);
            stack.CheckTransactionCount(1, 0, 1, 0);
        }

        [Test]
        public void TestBeginDiscard()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            stack.Stack.BeginTransaction();
            int orderCheck = 0;
            var action1 = new SimpleActionItem(); action1.OnUndo += () => { orderCheck++; };
            var action2 = new SimpleActionItem(); action2.OnUndo += () => { orderCheck++; };
            var action3 = new SimpleActionItem(); action3.OnUndo += () => { orderCheck++; };
            stack.Stack.Add(action1);
            stack.Stack.Add(action2);
            stack.Stack.Add(action3);
            var current = stack.Stack.GetCurrentTransactions();
            Assert.AreEqual(3, current.Count);
            stack.Stack.DiscardTransaction();
            Assert.AreEqual(0, orderCheck);
            Assert.Throws<InvalidOperationException>(() => stack.Stack.GetCurrentTransactions());
            stack.CheckRaiseCount(0, 0, 0, 0, 0);
            stack.CheckTransactionCount(1, 0, 0, 1);
        }

        [Test]
        public void TestUsingBeginDiscard()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            int orderCheck = 0;
            using (stack.Stack.BeginDiscardTransaction())
            {
                var action1 = new SimpleActionItem(); action1.OnUndo += () => { orderCheck++; };
                var action2 = new SimpleActionItem(); action2.OnUndo += () => { orderCheck++; };
                var action3 = new SimpleActionItem(); action3.OnUndo += () => { orderCheck++; };
                stack.Stack.Add(action1);
                stack.Stack.Add(action2);
                stack.Stack.Add(action3);
                var current = stack.Stack.GetCurrentTransactions();
                Assert.AreEqual(3, current.Count);
            }
            Assert.AreEqual(0, orderCheck);
            Assert.Throws<InvalidOperationException>(() => stack.Stack.GetCurrentTransactions());
            stack.CheckRaiseCount(0, 0, 0, 0, 0);
            stack.CheckTransactionCount(1, 0, 0, 1);
        }

        [Test]
        public void TestNestedBeginEnd()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            stack.Stack.BeginTransaction();
            stack.Stack.Add(new SimpleActionItem());
            stack.Stack.BeginTransaction();
            stack.Stack.Add(new SimpleActionItem());
            stack.Stack.Add(new SimpleActionItem());
            var current = stack.Stack.GetCurrentTransactions();
            Assert.AreEqual(2, current.Count);
            stack.Stack.EndTransaction("Test");
            stack.CheckRaiseCount(0, 0, 0, 0, 0);
            stack.CheckTransactionCount(2, 1, 0, 0);
            stack.Stack.Add(new SimpleActionItem());
            current = stack.Stack.GetCurrentTransactions();
            Assert.AreEqual(3, current.Count);
            stack.Stack.EndTransaction("Test");
            Assert.Throws<InvalidOperationException>(() => stack.Stack.GetCurrentTransactions());
            stack.CheckRaiseCount(1, 0, 0, 0, 0);
            stack.CheckTransactionCount(2, 2, 0, 0);
        }

        [Test]
        public void TestNestedBeginDiscardEnd()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            stack.Stack.BeginTransaction();
            stack.Stack.Add(new SimpleActionItem());
            stack.Stack.BeginTransaction();
            stack.Stack.Add(new SimpleActionItem());
            stack.Stack.Add(new SimpleActionItem());
            stack.Stack.Add(new SimpleActionItem());
            var current = stack.Stack.GetCurrentTransactions();
            Assert.AreEqual(3, current.Count);
            stack.Stack.DiscardTransaction();
            stack.CheckRaiseCount(0, 0, 0, 0, 0);
            stack.CheckTransactionCount(2, 0, 0, 1);
            stack.Stack.Add(new SimpleActionItem());
            current = stack.Stack.GetCurrentTransactions();
            Assert.AreEqual(2, current.Count);
            stack.Stack.EndTransaction("Test");
            Assert.Throws<InvalidOperationException>(() => stack.Stack.GetCurrentTransactions());
            stack.CheckRaiseCount(1, 0, 0, 0, 0);
            stack.CheckTransactionCount(2, 1, 0, 1);
        }

        [Test]
        public void TestNestedBeginCancelEnd()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            int orderCheck = 0;
            stack.Stack.BeginTransaction();
            stack.Stack.Add(new SimpleActionItem());
            stack.Stack.BeginTransaction();
            var action1 = new SimpleActionItem(); action1.OnUndo += () => { Assert.AreEqual(2, orderCheck); orderCheck++; };
            var action2 = new SimpleActionItem(); action2.OnUndo += () => { Assert.AreEqual(1, orderCheck); orderCheck++; };
            var action3 = new SimpleActionItem(); action3.OnUndo += () => { Assert.AreEqual(0, orderCheck); orderCheck++; };
            stack.Stack.Add(action1);
            stack.Stack.Add(action2);
            stack.Stack.Add(action3);
            var current = stack.Stack.GetCurrentTransactions();
            Assert.AreEqual(3, current.Count);
            stack.Stack.CancelTransaction();
            stack.CheckRaiseCount(0, 0, 0, 0, 0);
            stack.CheckTransactionCount(2, 0, 1, 0);
            stack.Stack.Add(new SimpleActionItem());
            current = stack.Stack.GetCurrentTransactions();
            Assert.AreEqual(2, current.Count);
            stack.Stack.EndTransaction("Test");
            Assert.AreEqual(3, orderCheck);
            Assert.Throws<InvalidOperationException>(() => stack.Stack.GetCurrentTransactions());
            stack.CheckRaiseCount(1, 0, 0, 0, 0);
            stack.CheckTransactionCount(2, 1, 1, 0);
        }

        [Test]
        public void TestBeginEvent()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            stack.Stack.TransactionStarted += (sender, e) =>
            {
                Assert.IsInstanceOf(typeof(TransactionalActionStack), sender);
                var localStack = (TransactionalActionStack)sender;
                Assert.DoesNotThrow(() => localStack.GetCurrentTransactions());
                var current = localStack.GetCurrentTransactions();
                Assert.AreEqual(0, current.Count);
            };
            stack.Stack.BeginTransaction();
            stack.CheckTransactionCount(1, 0, 0, 0);
        }

        [Test]
        public void TestEndEvent()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            var action1 = new SimpleActionItem();
            var action2 = new SimpleActionItem();
            stack.Stack.TransactionEnded += (sender, e) =>
            {
                Assert.IsInstanceOf(typeof(TransactionalActionStack), sender);
                var localStack = (TransactionalActionStack)sender;
                Assert.AreEqual(1, e.ActionItems.Length);
                Assert.IsInstanceOf(typeof(AggregateActionItem), e.ActionItems.First());
                var localAction = (AggregateActionItem)e.ActionItems.First();
                var items = localAction.ActionItems;
                Assert.AreEqual(action1, items.First());
                Assert.AreEqual(action2, items.Skip(1).First());
                Assert.Throws<InvalidOperationException>(() => localStack.GetCurrentTransactions());
            };
            stack.Stack.BeginTransaction();
            stack.Stack.Add(action1);
            stack.Stack.Add(action2);
            stack.Stack.EndTransaction("");
            stack.CheckTransactionCount(1, 1, 0, 0);
        }

        [Test]
        public void TestCancelEvent()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            var action1 = new SimpleActionItem();
            var action2 = new SimpleActionItem();
            stack.Stack.TransactionEnded += (sender, e) =>
            {
                Assert.IsInstanceOf(typeof(TransactionalActionStack), sender);
                var localStack = (TransactionalActionStack)sender;
                Assert.AreEqual(2, e.ActionItems.Length);
                Assert.AreEqual(action1, e.ActionItems.First());
                Assert.AreEqual(action2, e.ActionItems.Skip(1).First());
                Assert.DoesNotThrow(() => localStack.GetCurrentTransactions());
                Assert.Throws<InvalidOperationException>(() => localStack.GetCurrentTransactions());
            };
            stack.Stack.BeginTransaction();
            stack.Stack.Add(action1);
            stack.Stack.Add(action2);
            stack.Stack.CancelTransaction();
            stack.CheckTransactionCount(1, 0, 1, 0);
        }

        [Test]
        public void TestDiscardEvent()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            var action1 = new SimpleActionItem();
            var action2 = new SimpleActionItem();
            stack.Stack.TransactionEnded += (sender, e) =>
            {
                Assert.IsInstanceOf(typeof(TransactionalActionStack), sender);
                var localStack = (TransactionalActionStack)sender;
                Assert.AreEqual(2, e.ActionItems.Length);
                Assert.AreEqual(action1, e.ActionItems.First());
                Assert.AreEqual(action2, e.ActionItems.Skip(1).First());
                Assert.DoesNotThrow(() => localStack.GetCurrentTransactions());
                Assert.Throws<InvalidOperationException>(() => localStack.GetCurrentTransactions());
            };
            stack.Stack.BeginTransaction();
            stack.Stack.Add(action1);
            stack.Stack.Add(action2);
            stack.Stack.DiscardTransaction();
            stack.CheckTransactionCount(1, 0, 0, 1);
        }

        [Test]
        public void TestDontReverseOrder()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            int undoOrderCheck = 0;
            int redoOrderCheck = 0;
            var action1 = new SimpleActionItem(); action1.OnUndo += () => { Assert.AreEqual(0, undoOrderCheck); undoOrderCheck++; }; action1.OnRedo += () => { Assert.AreEqual(0, redoOrderCheck); redoOrderCheck++; };
            var action2 = new SimpleActionItem(); action2.OnUndo += () => { Assert.AreEqual(1, undoOrderCheck); undoOrderCheck++; }; action2.OnRedo += () => { Assert.AreEqual(1, redoOrderCheck); redoOrderCheck++; };
            stack.Stack.BeginTransaction();
            stack.Stack.Add(action1);
            stack.Stack.Add(action2);
            stack.Stack.EndTransaction("Test", false);
            Assert.AreEqual(0, undoOrderCheck);
            Assert.AreEqual(0, redoOrderCheck);
            stack.Stack.Undo();
            Assert.AreEqual(2, undoOrderCheck);
            Assert.AreEqual(0, redoOrderCheck);
            stack.Stack.Redo();
            Assert.AreEqual(2, undoOrderCheck);
            Assert.AreEqual(2, redoOrderCheck);
        }

        [Test]
        public void TestDontReverseNestedOrder()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            int undoOrderCheck = 0;
            int redoOrderCheck = 0;
            var action1 = new SimpleActionItem(); action1.OnUndo += () => { Assert.AreEqual(2, undoOrderCheck); undoOrderCheck++; }; action1.OnRedo += () => { Assert.AreEqual(0, redoOrderCheck); redoOrderCheck++; };
            var action2 = new SimpleActionItem(); action2.OnUndo += () => { Assert.AreEqual(3, undoOrderCheck); undoOrderCheck++; }; action2.OnRedo += () => { Assert.AreEqual(1, redoOrderCheck); redoOrderCheck++; };
            var action3 = new SimpleActionItem(); action3.OnUndo += () => { Assert.AreEqual(1, undoOrderCheck); undoOrderCheck++; }; action3.OnRedo += () => { Assert.AreEqual(2, redoOrderCheck); redoOrderCheck++; };
            var action4 = new SimpleActionItem(); action4.OnUndo += () => { Assert.AreEqual(0, undoOrderCheck); undoOrderCheck++; }; action4.OnRedo += () => { Assert.AreEqual(3, redoOrderCheck); redoOrderCheck++; };
            stack.Stack.BeginTransaction();
            stack.Stack.BeginTransaction();
            stack.Stack.Add(action1);
            stack.Stack.Add(action2);
            stack.Stack.EndTransaction("Test", false);
            stack.Stack.BeginTransaction();
            stack.Stack.Add(action3);
            stack.Stack.Add(action4);
            stack.Stack.EndTransaction("Test");
            stack.Stack.EndTransaction("Test");
            Assert.AreEqual(0, undoOrderCheck);
            Assert.AreEqual(0, redoOrderCheck);
            stack.Stack.Undo();
            Assert.AreEqual(4, undoOrderCheck);
            Assert.AreEqual(0, redoOrderCheck);
            stack.Stack.Redo();
            Assert.AreEqual(4, undoOrderCheck);
            Assert.AreEqual(4, redoOrderCheck);
        }
        [Test]
        public void TestDontReverseSingleNestedOrder()
        {
            var stack = new TransactionalActionStackTestContainer(5);
            int undoOrderCheck = 0;
            int redoOrderCheck = 0;
            var action1 = new SimpleActionItem(); action1.OnUndo += () => { Assert.AreEqual(0, undoOrderCheck); undoOrderCheck++; }; action1.OnRedo += () => { Assert.AreEqual(0, redoOrderCheck); redoOrderCheck++; };
            var action2 = new SimpleActionItem(); action2.OnUndo += () => { Assert.AreEqual(1, undoOrderCheck); undoOrderCheck++; }; action2.OnRedo += () => { Assert.AreEqual(1, redoOrderCheck); redoOrderCheck++; };
            stack.Stack.BeginTransaction();
            stack.Stack.BeginTransaction();
            stack.Stack.Add(action1);
            stack.Stack.Add(action2);
            stack.Stack.EndTransaction("Test", false);
            stack.Stack.EndTransaction("Test");
            Assert.AreEqual(0, undoOrderCheck);
            Assert.AreEqual(0, redoOrderCheck);
            stack.Stack.Undo();
            Assert.AreEqual(2, undoOrderCheck);
            Assert.AreEqual(0, redoOrderCheck);
            stack.Stack.Redo();
            Assert.AreEqual(2, undoOrderCheck);
            Assert.AreEqual(2, redoOrderCheck);
        }
    }
}
