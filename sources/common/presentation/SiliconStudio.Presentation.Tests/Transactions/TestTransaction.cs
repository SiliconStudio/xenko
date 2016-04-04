using NUnit.Framework;
using SiliconStudio.Presentation.Tests.Transactions.Helpers;
using SiliconStudio.Presentation.Transactions;

namespace SiliconStudio.Presentation.Tests.Transactions
{
    [TestFixture]
    public class TestTransaction
    {
        [Test]
        public void TestEmptyTransaction()
        {
            var stack = TransactionStackFactory.Create(5);
            using (stack.CreateTransaction())
            {
                // Empty transaction
            }
            Assert.AreEqual(true, stack.IsEmpty);
            Assert.AreEqual(false, stack.CanRollback);
            Assert.AreEqual(false, stack.CanRollforward);
            Assert.Throws<TransactionException>(() => stack.Rollback());
        }

        [Test]
        public void TestSingleOperationTransaction()
        {
            var stack = TransactionStackFactory.Create(5);
            SimpleOperation operation;
            using (stack.CreateTransaction())
            {
                operation = new SimpleOperation();
                stack.PushOperation(new SimpleOperation());
            }
            Assert.AreEqual(false, stack.IsEmpty);
            Assert.AreEqual(true, stack.CanRollback);
            Assert.AreEqual(false, stack.CanRollforward);
            Assert.AreEqual(true, operation.IsDone);
            Assert.AreEqual(0, operation.RollbackCount);
            Assert.AreEqual(0, operation.RollforwardCount);
        }

        [Test]
        public void TestSingleOperationTransactionRollback()
        {
            var stack = TransactionStackFactory.Create(5);
            SimpleOperation operation;
            using (stack.CreateTransaction())
            {
                operation = new SimpleOperation();
                stack.PushOperation(operation);
            }
            // Above code must be similar to TestSingleOperationTransaction
            stack.Rollback();
            Assert.AreEqual(false, stack.IsEmpty);
            Assert.AreEqual(false, stack.CanRollback);
            Assert.AreEqual(true, stack.CanRollforward);
            Assert.AreEqual(false, operation.IsDone);
            Assert.AreEqual(1, operation.RollbackCount);
            Assert.AreEqual(0, operation.RollforwardCount);
        }

        [Test]
        public void TestSingleOperationTransactionRollforward()
        {
            var stack = TransactionStackFactory.Create(5);
            SimpleOperation operation;
            using (stack.CreateTransaction())
            {
                operation = new SimpleOperation();
                stack.PushOperation(operation);
            }
            stack.Rollback();
            // Above code must be similar to TestSingleOperationTransactionRollback
            stack.Rollforward();
            Assert.AreEqual(false, stack.IsEmpty);
            Assert.AreEqual(true, stack.CanRollback);
            Assert.AreEqual(false, stack.CanRollforward);
            Assert.AreEqual(true, operation.IsDone);
            Assert.AreEqual(1, operation.RollbackCount);
            Assert.AreEqual(1, operation.RollforwardCount);
        }

        [Test]
        public void TestMultipleOperationsTransaction()
        {
            var stack = TransactionStackFactory.Create(5);
            var counter = new OrderedOperation.Counter();
            OrderedOperation[] operations = new OrderedOperation[4];
            using (stack.CreateTransaction())
            {
                for (int i = 0; i < operations.Length; ++i)
                {
                    var operation = new OrderedOperation(counter, 0, operations.Length - i - 1);
                    stack.PushOperation(operation);
                }
            }
            Assert.AreEqual(false, stack.IsEmpty);
            Assert.AreEqual(true, stack.CanRollback);
            Assert.AreEqual(false, stack.CanRollforward);
        }

        [Test]
        public void TestMultipleOperationsTransactionRollback()
        {
            var stack = TransactionStackFactory.Create(5);
            var counter = new OrderedOperation.Counter();
            OrderedOperation[] operations = new OrderedOperation[4];
            using (stack.CreateTransaction())
            {
                for (var i = 0; i < operations.Length; ++i)
                {
                    operations[i] = new OrderedOperation(counter, i, operations.Length);
                    stack.PushOperation(operations[i]);
                }
            }
            // Above code must be similar to TestMultipleOperationsTransaction
            stack.Rollback();
            Assert.AreEqual(false, stack.IsEmpty);
            Assert.AreEqual(false, stack.CanRollback);
            Assert.AreEqual(true, stack.CanRollforward);
            Assert.AreEqual(operations.Length, counter.Value);
            foreach (var operation in operations)
            {
                Assert.AreEqual(false, operation.IsDone);
                Assert.AreEqual(1, operation.RollbackCount);
                Assert.AreEqual(0, operation.RollforwardCount);
            }
        }

        [Test]
        public void TestMultipleOperationsTransactionRollforward()
        {
            var stack = TransactionStackFactory.Create(5);
            var counter = new OrderedOperation.Counter();
            OrderedOperation[] operations = new OrderedOperation[4];
            using (stack.CreateTransaction())
            {
                for (var i = 0; i < operations.Length; ++i)
                {
                    operations[i] = new OrderedOperation(counter, i, operations.Length);
                    stack.PushOperation(operations[i]);
                }
            }
            stack.Rollback();
            // Above code must be similar to TestMultipleOperationsTransactionRollback
            counter.Reset();
            stack.Rollforward();
            Assert.AreEqual(false, stack.IsEmpty);
            Assert.AreEqual(true, stack.CanRollback);
            Assert.AreEqual(false, stack.CanRollforward);
            Assert.AreEqual(operations.Length, counter.Value);
            foreach (var operation in operations)
            {
                Assert.AreEqual(true, operation.IsDone);
                Assert.AreEqual(1, operation.RollbackCount);
                Assert.AreEqual(1, operation.RollforwardCount);
            }
        }

        public void TestClear()
        {
            var stack = TransactionStackFactory.Create(5);
            var counter = new OrderedOperation.Counter();
            OrderedOperation[] operations = new OrderedOperation[4];
            using (stack.CreateTransaction())
            {
                for (int i = 0; i < operations.Length; ++i)
                {
                    var operation = new OrderedOperation(counter, 0, operations.Length - i - 1);
                    stack.PushOperation(operation);
                }
            }
            stack.Clear();
            Assert.AreEqual(false, stack.CanRollback);
            Assert.AreEqual(false, stack.CanRollforward);
            Assert.AreEqual(5, stack.Capacity);
            Assert.AreEqual(true, stack.IsEmpty);
            Assert.AreEqual(false, stack.IsFull);
        }
    }
}
