using NUnit.Framework;
using SiliconStudio.ActionStack.Tests.Helpers;

namespace SiliconStudio.ActionStack.Tests
{
    [TestFixture]
    class TestDirtiable
    {
        [Test]
        public void TestDoAction()
        {
            var actionStack = new TransactionalActionStack(5);
            using (new DirtiableManager(actionStack))
            {
                var dirtiable = new SimpleDirtiable();
                var action = new SimpleDirtiableActionItem(dirtiable.Yield());
                actionStack.Add(action);
                Assert.AreEqual(true, dirtiable.IsDirty);
            }
        }

        [Test]
        public void TestDoAndSave()
        {
            var actionStack = new TransactionalActionStack(5);
            using (var manager = new DirtiableManager(actionStack))
            {
                var dirtiable = new SimpleDirtiable();
                var action = new SimpleDirtiableActionItem(dirtiable.Yield());
                actionStack.Add(action);
                Assert.AreEqual(true, dirtiable.IsDirty);
                actionStack.CreateSavePoint(true);
                manager.NotifySave();
                Assert.AreEqual(false, dirtiable.IsDirty);
            }
        }

        [Test]
        public void TestUndo()
        {
            var actionStack = new TransactionalActionStack(5);
            using (new DirtiableManager(actionStack))
            {
                var dirtiable = new SimpleDirtiable();
                var action = new SimpleDirtiableActionItem(dirtiable.Yield());
                actionStack.Add(action);
                Assert.AreEqual(true, dirtiable.IsDirty);
                actionStack.Undo();
                Assert.AreEqual(false, dirtiable.IsDirty);
            }
        }

        [Test]
        public void TestRedo()
        {
            var actionStack = new TransactionalActionStack(5);
            using (new DirtiableManager(actionStack))
            {
                var dirtiable = new SimpleDirtiable();
                var action = new SimpleDirtiableActionItem(dirtiable.Yield());
                actionStack.Add(action);
                Assert.AreEqual(true, dirtiable.IsDirty);
                actionStack.Undo();
                actionStack.Redo();
                Assert.AreEqual(true, dirtiable.IsDirty);
            }
        }

        [Test]
        public void TestSaveUndoSaveRedo()
        {
            var actionStack = new TransactionalActionStack(5);
            using (var manager = new DirtiableManager(actionStack))
            {
                var dirtiable = new SimpleDirtiable();
                var action = new SimpleDirtiableActionItem(dirtiable.Yield());
                actionStack.Add(action);
                Assert.AreEqual(true, dirtiable.IsDirty);
                actionStack.CreateSavePoint(true);
                manager.NotifySave();
                Assert.AreEqual(false, dirtiable.IsDirty);
                actionStack.Undo();
                Assert.AreEqual(true, dirtiable.IsDirty);
                actionStack.CreateSavePoint(true);
                manager.NotifySave();
                Assert.AreEqual(false, dirtiable.IsDirty);
                actionStack.Redo();
                Assert.AreEqual(true, dirtiable.IsDirty);
                actionStack.CreateSavePoint(true);
                manager.NotifySave();
                Assert.AreEqual(false, dirtiable.IsDirty);
            }
        }
    }
}