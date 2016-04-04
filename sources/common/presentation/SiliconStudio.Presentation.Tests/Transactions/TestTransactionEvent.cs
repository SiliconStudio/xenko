using NUnit.Framework;
using SiliconStudio.Presentation.Tests.Transactions.Helpers;
using SiliconStudio.Presentation.Transactions;

namespace SiliconStudio.Presentation.Tests.Transactions
{
    [TestFixture]
    public class TestTransactionEvent
    {
        [Test]
        public void TestTransactionCompleted()
        {
            var stack = TransactionStackFactory.Create(5);
            var raiseCount = 0;
            var expectedRaiseCount = 0;
            // ReSharper disable once AccessToModifiedClosure - this is what I want to achieve
            stack.TransactionCompleted += (sender, e) => Assert.AreEqual(expectedRaiseCount, ++raiseCount);
            for (var j = 0; j < 8; ++j)
            {
                ++expectedRaiseCount;
                using (stack.CreateTransaction())
                {
                    for (var i = 0; i < 5; ++i)
                    {
                        var operation = new SimpleOperation();
                        stack.PushOperation(operation);
                    }
                }
            }
            Assert.AreEqual(8, expectedRaiseCount);
            Assert.AreEqual(8, raiseCount);
        }

        [Test]
        public void TestTransactionCleared()
        {
            var stack = TransactionStackFactory.Create(5);
            var raiseCount = 0;
            var expectedRaiseCount = 0;
            // ReSharper disable once AccessToModifiedClosure - this is what I want to achieve
            stack.Cleared += (sender, e) => Assert.AreEqual(expectedRaiseCount, ++raiseCount);
            for (var j = 0; j < 8; ++j)
            {
                using (stack.CreateTransaction())
                {
                    for (var i = 0; i < 5; ++i)
                    {
                        var operation = new SimpleOperation();
                        stack.PushOperation(operation);
                    }
                }
            }
            ++expectedRaiseCount;
            stack.Clear();
            Assert.AreEqual(1, expectedRaiseCount);
            Assert.AreEqual(1, raiseCount);
        }

        [Test]
        public void TestTransactionRollbacked()
        {
            var stack = TransactionStackFactory.Create(5);
            var raiseCount = 0;
            var expectedRaiseCount = 0;
            // ReSharper disable once AccessToModifiedClosure - this is what I want to achieve
            stack.TransactionRollbacked += (sender, e) => Assert.AreEqual(expectedRaiseCount, ++raiseCount);
            for (var j = 0; j < stack.Capacity + 3; ++j)
            {
                using (stack.CreateTransaction())
                {
                    for (var i = 0; i < 3; ++i)
                    {
                        var operation = new SimpleOperation();
                        stack.PushOperation(operation);
                    }
                }
            }
            for (var j = 0; j < stack.Capacity; ++j)
            {
                ++expectedRaiseCount;
                stack.Rollback();
            }
            Assert.AreEqual(stack.Capacity, expectedRaiseCount);
            Assert.AreEqual(stack.Capacity, raiseCount);
            Assert.AreEqual(5, stack.Capacity);
        }

        [Test]
        public void TestTransactionRollforwarded()
        {
            var stack = TransactionStackFactory.Create(5);
            var raiseCount = 0;
            var expectedRaiseCount = 0;
            // ReSharper disable once AccessToModifiedClosure - this is what I want to achieve
            stack.TransactionRollforwarded += (sender, e) => Assert.AreEqual(expectedRaiseCount, ++raiseCount);
            for (var j = 0; j < stack.Capacity + 3; ++j)
            {
                using (stack.CreateTransaction())
                {
                    for (var i = 0; i < 3; ++i)
                    {
                        var operation = new SimpleOperation();
                        stack.PushOperation(operation);
                    }
                }
            }
            for (var j = 0; j < stack.Capacity; ++j)
            {
                stack.Rollback();
            }
            for (var j = 0; j < stack.Capacity; ++j)
            {
                ++expectedRaiseCount;
                stack.Rollforward();
            }
            Assert.AreEqual(stack.Capacity, expectedRaiseCount);
            Assert.AreEqual(stack.Capacity, raiseCount);
            Assert.AreEqual(5, stack.Capacity);
        }

        // TODO: tests for TransactionDiscarded (discard one, discard many, etc.)
    }
}
