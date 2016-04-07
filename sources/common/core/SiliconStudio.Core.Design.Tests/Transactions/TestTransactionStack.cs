using NUnit.Framework;
using SiliconStudio.Core.Transactions;

namespace SiliconStudio.Core.Design.Tests.Transactions
{
    [TestFixture]
    public class TestTransactionStack
    {
        [Test]
        public void TestConstruction()
        {
            var stack = TransactionStackFactory.Create(5);
            Assert.AreEqual(false, stack.CanRollback);
            Assert.AreEqual(false, stack.CanRollforward);
            Assert.AreEqual(5, stack.Capacity);
            Assert.AreEqual(true, stack.IsEmpty);
            Assert.AreEqual(false, stack.IsFull);
        }

        [Test]
        public void TestOverCapacity()
        {
            var stack = (TransactionStack)TransactionStackFactory.Create(5);
            var operations = new SimpleOperation[6];
            for (var i = 0; i < 5; ++i)
            {
                Assert.AreEqual(false, stack.IsFull);
                using (stack.CreateTransaction())
                {
                    operations[i] = new SimpleOperation();
                    stack.PushOperation(operations[i]);
                }
            }
            Assert.AreEqual(true, stack.IsFull);
            Assert.AreEqual(5, stack.Capacity);
            for (var i = 0; i < 5; ++i)
            {
                Assert.AreEqual(operations[i], ((Transaction)stack.Transactions[i]).Operations[0]);
            }
            using (stack.CreateTransaction())
            {
                operations[5] = new SimpleOperation();
                stack.PushOperation(operations[5]);
            }
            Assert.AreEqual(5, stack.Transactions.Count);
            Assert.AreEqual(5, stack.Capacity);
            Assert.AreEqual(true, operations[0].IsFrozen);
            for (var i = 0; i < 5; ++i)
            {
                Assert.AreEqual(operations[i+1], ((Transaction)stack.Transactions[i]).Operations[0]);
            }
        }

        [Test]
        public void TestZeroCapacity()
        {
            var stack = (TransactionStack)TransactionStackFactory.Create(0);
            SimpleOperation operation;
            Assert.AreEqual(false, stack.CanRollback);
            Assert.AreEqual(false, stack.CanRollforward);
            Assert.AreEqual(0, stack.Capacity);
            Assert.AreEqual(true, stack.IsFull);
            Assert.AreEqual(true, stack.IsEmpty);

            using (stack.CreateTransaction())
            {
                operation = new SimpleOperation();
                stack.PushOperation(operation);
            }
            Assert.AreEqual(true, operation.IsFrozen);
            using (stack.CreateTransaction())
            {
                operation = new SimpleOperation();
                stack.PushOperation(operation);
            }
            Assert.AreEqual(true, operation.IsFrozen);
            Assert.AreEqual(false, stack.CanRollback);
            Assert.AreEqual(false, stack.CanRollforward);
            Assert.AreEqual(0, stack.Capacity);
            Assert.AreEqual(true, stack.IsFull);
            Assert.AreEqual(true, stack.IsEmpty);
        }
    }
}
