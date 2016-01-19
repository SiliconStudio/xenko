using System.Linq;
using NUnit.Framework;
using SiliconStudio.ActionStack.Tests.Helpers;

namespace SiliconStudio.ActionStack.Tests
{
    [TestFixture]
    class TestActionStack
    {
        [Test]
        public void TestConstruction()
        {
            var stack = new ActionStackTestContainer(5);
            Assert.AreEqual(5, stack.Stack.Capacity);
            Assert.AreEqual(false, stack.Stack.CanUndo);
            Assert.AreEqual(false, stack.Stack.CanRedo);
            Assert.AreEqual(0, stack.Stack.ActionItems.Count());
        }

        [Test]
        public void TestAddAction()
        {
            var stack = new ActionStackTestContainer(5);
            var action = new SimpleActionItem();
            stack.Stack.Add(action);
            Assert.AreEqual(true, stack.Stack.CanUndo);
            Assert.AreEqual(false, stack.Stack.CanRedo);
            Assert.AreEqual(1, stack.Stack.ActionItems.Count());
            Assert.AreEqual(action, stack.Stack.ActionItems.First());
            stack.CheckRaiseCount(1, 0, 0, 0, 0);
        }

        [Test]
        public void TestActionItemsAddedOneAction()
        {
            var stack = new ActionStackTestContainer(5);
            var action = new SimpleActionItem();
            stack.Stack.ActionItemsAdded += (sender, e) =>
            {
                Assert.AreEqual(1, e.ActionItems.Length);
                Assert.AreEqual(action, e.ActionItems.First());
                Assert.IsInstanceOf(typeof(ActionStack), sender);
                var localStack = (ActionStack)sender;
                Assert.AreEqual(true, localStack.CanUndo);
                Assert.AreEqual(false, localStack.CanRedo);
            };
            stack.Stack.Add(action);
            Assert.AreEqual(1, stack.Stack.ActionItems.Count());
            stack.CheckRaiseCount(1, 0, 0, 0, 0);
        }

        [Test]
        public void TestActionItemsAddedTwoActions()
        {
            var stack = new ActionStackTestContainer(5);
            var actions = new[] { new SimpleActionItem(), new SimpleActionItem() };
            stack.Stack.ActionItemsAdded += (sender, e) =>
            {
                Assert.AreEqual(2, e.ActionItems.Length);
                Assert.AreEqual(actions[0], e.ActionItems.First());
                Assert.AreEqual(actions[1], e.ActionItems.Skip(1).First());
                Assert.IsInstanceOf(typeof(ActionStack), sender);
                var localStack = (ActionStack)sender;
                Assert.AreEqual(true, localStack.CanUndo);
                Assert.AreEqual(false, localStack.CanRedo);
            };
            stack.Stack.AddRange(actions);
            Assert.AreEqual(2, stack.Stack.ActionItems.Count());
            stack.CheckRaiseCount(1, 0, 0, 0, 0);
        }

        [Test]
        public void TestActionItemsCleared()
        {
            var stack = new ActionStackTestContainer(5);
            stack.Stack.ActionItemsCleared += (sender, e) =>
            {
                var localStack = (ActionStack)sender;
                Assert.AreEqual(false, localStack.CanUndo);
                Assert.AreEqual(false, localStack.CanRedo);
            };
            stack.Stack.AddRange(new[] { new SimpleActionItem(), new SimpleActionItem() });
            stack.Stack.Clear();
            Assert.AreEqual(0, stack.Stack.ActionItems.Count());
            stack.CheckRaiseCount(1, 1, 0, 0, 0);
        }

        [Test]
        public void TestUndo()
        {
            var stack = new ActionStackTestContainer(5);
            var action = new SimpleActionItem();
            stack.Stack.Add(action);
            stack.Stack.Undo();
            Assert.AreEqual(false, action.IsDone);
            Assert.AreEqual(false, action.IsFrozen);
            Assert.AreEqual(false, action.IsSaved);
            Assert.AreEqual(false, stack.Stack.CanUndo);
            Assert.AreEqual(true, stack.Stack.CanRedo);
            Assert.AreEqual(1, stack.Stack.ActionItems.Count());
            Assert.AreEqual(action, stack.Stack.ActionItems.First());
            stack.CheckRaiseCount(1, 0, 0, 1, 0);
        }

        [Test]
        public void TestRedo()
        {
            var stack = new ActionStackTestContainer(5);
            var action = new SimpleActionItem();
            stack.Stack.Add(action);
            stack.Stack.Undo();
            stack.Stack.Redo();
            Assert.AreEqual(true, action.IsDone);
            Assert.AreEqual(false, action.IsFrozen);
            Assert.AreEqual(false, action.IsSaved);
            Assert.AreEqual(true, stack.Stack.CanUndo);
            Assert.AreEqual(false, stack.Stack.CanRedo);
            Assert.AreEqual(1, stack.Stack.ActionItems.Count());
            Assert.AreEqual(action, stack.Stack.ActionItems.First());
            stack.CheckRaiseCount(1, 0, 0, 1, 1);
        }

        [Test]
        public void TestUndoEvent()
        {
            var stack = new ActionStackTestContainer(5);
            var action = new SimpleActionItem();
            stack.Stack.Undone += (sender, e) =>
            {
                Assert.AreEqual(1, e.ActionItems.Length);
                Assert.AreEqual(action, e.ActionItems.First());
                Assert.IsInstanceOf(typeof(ActionStack), sender);
                var localStack = (ActionStack)sender;
                var localAction = e.ActionItems.First();
                Assert.AreEqual(false, localAction.IsDone);
                Assert.AreEqual(false, localAction.IsFrozen);
                Assert.AreEqual(false, localAction.IsSaved);
                Assert.AreEqual(false, localStack.CanUndo);
                Assert.AreEqual(true, localStack.CanRedo);
            };
            stack.Stack.Add(action);
            stack.Stack.Undo();
            stack.CheckRaiseCount(1, 0, 0, 1, 0);
        }

        [Test]
        public void TestRedoEvent()
        {
            var stack = new ActionStackTestContainer(5);
            var action = new SimpleActionItem();
            stack.Stack.Redone += (sender, e) =>
            {
                Assert.AreEqual(1, e.ActionItems.Length);
                Assert.AreEqual(action, e.ActionItems.First());
                Assert.IsInstanceOf(typeof(ActionStack), sender);
                var localStack = (ActionStack)sender;
                var localAction = e.ActionItems.First();
                Assert.AreEqual(true, localAction.IsDone);
                Assert.AreEqual(false, localAction.IsFrozen);
                Assert.AreEqual(false, localAction.IsSaved);
                Assert.AreEqual(true, localStack.CanUndo);
                Assert.AreEqual(false, localStack.CanRedo);
            };
            stack.Stack.Add(action);
            stack.Stack.Undo();
            stack.Stack.Redo();
            stack.CheckRaiseCount(1, 0, 0, 1, 1);
        }

        [Test]
        public void TestAddTwoActions()
        {
            var stack = new ActionStackTestContainer(5);
            var action1 = new SimpleActionItem();
            var action2 = new SimpleActionItem();
            stack.Stack.Add(action1);
            stack.Stack.Add(action2);
            Assert.AreEqual(true, stack.Stack.CanUndo);
            Assert.AreEqual(false, stack.Stack.CanRedo);
            Assert.AreEqual(2, stack.Stack.ActionItems.Count());
            Assert.AreEqual(action1, stack.Stack.ActionItems.First());
            Assert.AreEqual(action2, stack.Stack.ActionItems.Skip(1).First());
            stack.CheckRaiseCount(2, 0, 0, 0, 0);
        }

        [Test]
        public void TestAddTwoActionsAndUndoRedo()
        {
            var stack = new ActionStackTestContainer(5);
            var action1 = new SimpleActionItem();
            var action2 = new SimpleActionItem();
            stack.Stack.Add(action1);
            stack.Stack.Add(action2);
            stack.Stack.Undo();
            Assert.AreEqual(true, action1.IsDone);
            Assert.AreEqual(false, action1.IsFrozen);
            Assert.AreEqual(false, action1.IsSaved);
            Assert.AreEqual(false, action2.IsDone);
            Assert.AreEqual(false, action2.IsFrozen);
            Assert.AreEqual(false, action2.IsSaved);
            Assert.AreEqual(true, stack.Stack.CanUndo);
            Assert.AreEqual(true, stack.Stack.CanRedo);
            stack.Stack.Undo();
            Assert.AreEqual(false, action1.IsDone);
            Assert.AreEqual(false, action1.IsFrozen);
            Assert.AreEqual(false, action1.IsSaved);
            Assert.AreEqual(false, action2.IsDone);
            Assert.AreEqual(false, action2.IsFrozen);
            Assert.AreEqual(false, action2.IsSaved);
            Assert.AreEqual(false, stack.Stack.CanUndo);
            Assert.AreEqual(true, stack.Stack.CanRedo);
            stack.Stack.Redo();
            Assert.AreEqual(true, action1.IsDone);
            Assert.AreEqual(false, action1.IsFrozen);
            Assert.AreEqual(false, action1.IsSaved);
            Assert.AreEqual(false, action2.IsDone);
            Assert.AreEqual(false, action2.IsFrozen);
            Assert.AreEqual(false, action2.IsSaved);
            Assert.AreEqual(true, stack.Stack.CanUndo);
            Assert.AreEqual(true, stack.Stack.CanRedo);
            stack.Stack.Redo();
            Assert.AreEqual(true, action1.IsDone);
            Assert.AreEqual(false, action1.IsFrozen);
            Assert.AreEqual(false, action1.IsSaved);
            Assert.AreEqual(true, action2.IsDone);
            Assert.AreEqual(false, action2.IsFrozen);
            Assert.AreEqual(false, action2.IsSaved);
            Assert.AreEqual(true, stack.Stack.CanUndo);
            Assert.AreEqual(false, stack.Stack.CanRedo);
            stack.CheckRaiseCount(2, 0, 0, 2, 2);
        }

        [Test]
        public void TestSwallowActions()
        {
            var stack = new ActionStackTestContainer(2);
            var action1 = new SimpleActionItem();
            var action2 = new SimpleActionItem();
            var action3 = new SimpleActionItem();
            stack.Stack.Add(action1);
            stack.Stack.Add(action2);
            stack.Stack.Add(action3);
            Assert.AreEqual(true, stack.Stack.CanUndo);
            Assert.AreEqual(false, stack.Stack.CanRedo);
            Assert.AreEqual(true, action1.IsFrozen);
            Assert.AreEqual(false, action2.IsFrozen);
            Assert.AreEqual(false, action3.IsFrozen);
            Assert.AreEqual(2, stack.Stack.ActionItems.Count());
            Assert.AreEqual(action2, stack.Stack.ActionItems.First());
            Assert.AreEqual(action3, stack.Stack.ActionItems.Skip(1).First());
            stack.CheckRaiseCount(3, 0, 1, 0, 0);
        }

        [Test]
        public void TestSwallowAndUndoActions()
        {
            var stack = new ActionStackTestContainer(2);
            var action1 = new SimpleActionItem();
            var action2 = new SimpleActionItem();
            var action3 = new SimpleActionItem();
            var action4 = new SimpleActionItem();
            stack.Stack.Add(action1);
            stack.Stack.Add(action2);
            stack.Stack.Add(action3);
            stack.CheckRaiseCount(3, 0, 1, 0, 0);
            stack.Stack.Undo();
            stack.Stack.Add(action4);
            Assert.AreEqual(true, stack.Stack.CanUndo);
            Assert.AreEqual(false, stack.Stack.CanRedo);
            Assert.AreEqual(2, stack.Stack.ActionItems.Count());
            Assert.AreEqual(action2, stack.Stack.ActionItems.First());
            Assert.AreEqual(action4, stack.Stack.ActionItems.Skip(1).First());
            stack.CheckRaiseCount(4, 0, 2, 1, 0);
        }

        [Test]
        public void TestDisbranchActions()
        {
            // Partially undo the stack
            var stack = new ActionStackTestContainer(5);
            var action1 = new SimpleActionItem();
            var action2 = new SimpleActionItem();
            var action3 = new SimpleActionItem();
            var action4 = new SimpleActionItem();
            stack.Stack.Add(action1);
            stack.Stack.Add(action2);
            stack.Stack.Undo();
            stack.Stack.Add(action3);
            stack.Stack.Add(action4);
            Assert.AreEqual(true, stack.Stack.CanUndo);
            Assert.AreEqual(false, stack.Stack.CanRedo);
            Assert.AreEqual(3, stack.Stack.ActionItems.Count());
            Assert.AreEqual(action1, stack.Stack.ActionItems.First());
            Assert.AreEqual(action3, stack.Stack.ActionItems.Skip(1).First());
            Assert.AreEqual(action4, stack.Stack.ActionItems.Skip(2).First());
            stack.CheckRaiseCount(4, 0, 1, 1, 0);
        }

        [Test]
        public void TestDisbranchActions2()
        {
            // Completely undo the stack
            var stack = new ActionStackTestContainer(5);
            var action1 = new SimpleActionItem();
            var action2 = new SimpleActionItem();
            var action3 = new SimpleActionItem();
            var action4 = new SimpleActionItem();
            stack.Stack.Add(action1);
            stack.Stack.Add(action2);
            stack.Stack.Undo();
            stack.Stack.Undo();
            stack.Stack.Add(action3);
            stack.Stack.Add(action4);
            Assert.AreEqual(true, stack.Stack.CanUndo);
            Assert.AreEqual(false, stack.Stack.CanRedo);
            Assert.AreEqual(2, stack.Stack.ActionItems.Count());
            Assert.AreEqual(action3, stack.Stack.ActionItems.First());
            Assert.AreEqual(action4, stack.Stack.ActionItems.Skip(1).First());
            stack.CheckRaiseCount(4, 0, 1, 2, 0);
        }

        [Test]
        public void TestDisbranchEvent()
        {
            // Completely undo the stack
            var stack = new ActionStackTestContainer(5);
            var action1 = new SimpleActionItem();
            var action2 = new SimpleActionItem();
            stack.Stack.ActionItemsDiscarded += (sender, e) =>
            {
                Assert.AreEqual(ActionItemDiscardType.Disbranched, e.Type);
                Assert.AreEqual(1, e.ActionItems.Length);
                Assert.AreEqual(action1, e.ActionItems.First());
                Assert.IsInstanceOf(typeof(ActionStack), sender);
            };
            stack.Stack.Add(action1);
            stack.Stack.Undo();
            stack.Stack.Add(action2);
            Assert.AreEqual(1, stack.Stack.ActionItems.Count());
            Assert.AreEqual(action2, stack.Stack.ActionItems.First());
            stack.CheckRaiseCount(2, 0, 1, 1, 0);
        }

        [Test]
        public void TestSwallowEvent()
        {
            // Completely undo the stack
            var stack = new ActionStackTestContainer(1);
            var action1 = new SimpleActionItem();
            var action2 = new SimpleActionItem();
            stack.Stack.ActionItemsDiscarded += (sender, e) =>
            {
                Assert.AreEqual(ActionItemDiscardType.Swallowed, e.Type);
                Assert.AreEqual(1, e.ActionItems.Length);
                Assert.AreEqual(action1, e.ActionItems.First());
                Assert.IsInstanceOf(typeof(ActionStack), sender);
                var localStack = (ActionStack)sender;
                Assert.AreEqual(1, localStack.ActionItems.Count());
            };
            stack.Stack.Add(action1);
            stack.Stack.Add(action2);
            stack.CheckRaiseCount(2, 0, 1, 0, 0);
        }

        [Test]
        public void TestUndoRedoInProgressEvent()
        {
            // Completely undo the stack
            var stack = new ActionStackTestContainer(5);
            var action1 = new SimpleActionItem();
            var action2 = new AnonymousActionItem("test", Enumerable.Empty<IDirtiable>(), () => stack.Stack.Add(action1), () => { });
            stack.Stack.ActionItemsDiscarded += (sender, e) =>
            {
                Assert.AreEqual(ActionItemDiscardType.UndoRedoInProgress, e.Type);
                Assert.AreEqual(1, e.ActionItems.Length);
                Assert.AreEqual(action1, e.ActionItems.First());
                Assert.IsInstanceOf(typeof(ActionStack), sender);
                var localStack = (ActionStack)sender;
                var localAction = e.ActionItems.First();
                Assert.AreEqual(1, localStack.ActionItems.Count());
                Assert.AreEqual(action1, localAction);
            };
            stack.Stack.Add(action2);
            stack.Stack.Undo();
            stack.CheckRaiseCount(1, 0, 1, 1, 0);
        }

        [Test]
        public void TestSavePoint()
        {
            // Completely undo the stack
            var stack = new ActionStackTestContainer(5);
            var action1 = new SimpleActionItem();
            var action2 = new SimpleActionItem();
            stack.Stack.Add(action1);
            var savePoint = stack.Stack.CreateSavePoint(true);
            stack.Stack.Add(action2);
            Assert.AreEqual(action1.Identifier, savePoint.ActionItemIdentifier);
            Assert.AreEqual(true, action1.IsSaved);
            Assert.AreEqual(false, action2.IsSaved);
        }

        [Test]
        public void TestSavePointEquity()
        {
            // Completely undo the stack
            var stack = new ActionStackTestContainer(5);
            var action1 = new SimpleActionItem();
            stack.Stack.Add(action1);
            var savePoint1 = stack.Stack.CreateSavePoint(false);
            var savePoint2 = stack.Stack.CreateSavePoint(true);
            Assert.AreEqual(savePoint1, savePoint2);
        }
    }
}