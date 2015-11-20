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
        public void TestEvent()
        {
            var actionStack = new TransactionalActionStack(5);
            int eventCount = 0;
            using (new DirtiableManager(actionStack))
            {
                var dirtiable = new SimpleDirtiable();
                dirtiable.DirtinessUpdated += (sender, e) =>
                {
                    Assert.AreEqual(eventCount != 0, e.OldValue);
                    Assert.AreEqual(true, e.NewValue);
                    eventCount++;
                };
                Assert.AreEqual(false, dirtiable.IsDirty);
                actionStack.Add(new SimpleDirtiableActionItem(dirtiable.Yield()));
                Assert.AreEqual(true, dirtiable.IsDirty);
                actionStack.Add(new SimpleDirtiableActionItem(dirtiable.Yield()));
                Assert.AreEqual(true, dirtiable.IsDirty);
                Assert.AreEqual(2, eventCount);
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