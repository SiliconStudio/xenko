using System;
using System.Linq;
using NUnit.Framework;

namespace SiliconStudio.ActionStack.Tests
{
    [TestFixture]
    class TestAggregateActionItem
    {
        [Test]
        public void TestConstruction()
        {
            var action1 = new SimpleActionItem();
            var action2 = new SimpleActionItem();
            var action3 = new SimpleActionItem();
            var action = new AggregateActionItem("Test", action1, new AggregateActionItem("Test", action2, action3));
            var inner = action.GetInnerActionItems().ToList();
            Assert.AreEqual(2, action.ActionItems.Count);
            Assert.AreEqual(5, inner.Count);
            Assert.AreEqual(true, action.ContainsAction(action1));
            Assert.AreEqual(true, action.ContainsAction(action2));
            Assert.Contains(action1, inner);
            Assert.Contains(action2, inner);
            Assert.Contains(action3, inner);
        }

        [Test]
        public void TestConstructionExceptions()
        {
            // ReSharper disable ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new AggregateActionItem("", null));
            Assert.Throws<ArgumentException>(() => new AggregateActionItem(""));
            Assert.Throws<ArgumentException>(() => new AggregateActionItem("", new IActionItem[] { null }));
            // ReSharper restore ObjectCreationAsStatement
        }

        [Test]
        public void TestUndo()
        {
            int orderCheck = 0;
            var action1 = new SimpleActionItem(); action1.OnUndo += () => { Assert.AreEqual(2, orderCheck); orderCheck++; };
            var action2 = new SimpleActionItem(); action2.OnUndo += () => { Assert.AreEqual(1, orderCheck); orderCheck++; };
            var action3 = new SimpleActionItem(); action3.OnUndo += () => { Assert.AreEqual(0, orderCheck); orderCheck++; };
            var action = new AggregateActionItem("Test", action1, new AggregateActionItem("Test", action2, action3));
            action.Undo();
            Assert.AreEqual(false, action.IsDone);
            Assert.AreEqual(false, action1.IsDone);
            Assert.AreEqual(false, action2.IsDone);
            Assert.AreEqual(false, action3.IsDone);
        }

        [Test]
        public void TestRedo()
        {
            int orderCheck = 0;
            var action1 = new SimpleActionItem(); action1.OnRedo += () => { Assert.AreEqual(0, orderCheck); orderCheck++; };
            var action2 = new SimpleActionItem(); action2.OnRedo += () => { Assert.AreEqual(1, orderCheck); orderCheck++; };
            var action3 = new SimpleActionItem(); action3.OnRedo += () => { Assert.AreEqual(2, orderCheck); orderCheck++; };
            var action = new AggregateActionItem("Test", action1, new AggregateActionItem("Test", action2, action3));
            action.Undo();
            action.Redo();
            Assert.AreEqual(true, action.IsDone);
            Assert.AreEqual(true, action1.IsDone);
            Assert.AreEqual(true, action2.IsDone);
            Assert.AreEqual(true, action3.IsDone);
        }
    }
}