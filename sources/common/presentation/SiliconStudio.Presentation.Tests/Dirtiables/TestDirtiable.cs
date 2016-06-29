using NUnit.Framework;
using SiliconStudio.Core.Transactions;
using SiliconStudio.Presentation.Dirtiables;

namespace SiliconStudio.Presentation.Tests.Dirtiables
{
    [TestFixture]
    public class TestDirtiable
    {
        [Test]
        public void TestDoAction()
        {
            var stack = new TransactionStack(5);
            using (new DirtiableManager(stack))
            {
                var dirtiable = new SimpleDirtiable();
                using (stack.CreateTransaction())
                {
                    var operation = new SimpleDirtyingOperation(dirtiable.Yield());
                    stack.PushOperation(operation);
                }
                Assert.AreEqual(true, dirtiable.IsDirty);
            }
        }

        [Test]
        public void TestDoAndSave()
        {
            var stack = new TransactionStack(5);
            using (var manager = new DirtiableManager(stack))
            {
                var dirtiable = new SimpleDirtiable();
                var operation = new SimpleDirtyingOperation(dirtiable.Yield());
                using (stack.CreateTransaction())
                {
                    stack.PushOperation(operation);
                }
                Assert.AreEqual(true, dirtiable.IsDirty);
                manager.CreateSnapshot();
                Assert.AreEqual(false, dirtiable.IsDirty);
            }
        }

        [Test]
        public void TestUndo()
        {
            var stack = new TransactionStack(5);
            using (new DirtiableManager(stack))
            {
                var dirtiable = new SimpleDirtiable();
                var operation = new SimpleDirtyingOperation(dirtiable.Yield());
                using (stack.CreateTransaction())
                {
                    stack.PushOperation(operation);
                }
                Assert.AreEqual(true, dirtiable.IsDirty);
                stack.Rollback();
                Assert.AreEqual(false, dirtiable.IsDirty);
            }
        }

        [Test]
        public void TestRedo()
        {
            var stack = new TransactionStack(5);
            using (new DirtiableManager(stack))
            {
                var dirtiable = new SimpleDirtiable();
                var operation = new SimpleDirtyingOperation(dirtiable.Yield());
                using (stack.CreateTransaction())
                {
                    stack.PushOperation(operation);
                }
                Assert.AreEqual(true, dirtiable.IsDirty);
                stack.Rollback();
                stack.Rollforward();
                Assert.AreEqual(true, dirtiable.IsDirty);
            }
        }

        [Test]
        public void TestSaveUndoSaveRedo()
        {
            var stack = new TransactionStack(5);
            using (var manager = new DirtiableManager(stack))
            {
                var dirtiable = new SimpleDirtiable();
                var operation = new SimpleDirtyingOperation(dirtiable.Yield());
                using (stack.CreateTransaction())
                {
                    stack.PushOperation(operation);
                }
                Assert.AreEqual(true, dirtiable.IsDirty);
                manager.CreateSnapshot();
                Assert.AreEqual(false, dirtiable.IsDirty);
                stack.Rollback();
                Assert.AreEqual(true, dirtiable.IsDirty);
                manager.CreateSnapshot();
                Assert.AreEqual(false, dirtiable.IsDirty);
                stack.Rollforward();
                Assert.AreEqual(true, dirtiable.IsDirty);
                manager.CreateSnapshot();
                Assert.AreEqual(false, dirtiable.IsDirty);
            }
        }
    }
}
