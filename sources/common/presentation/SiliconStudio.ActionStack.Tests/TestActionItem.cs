// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using NUnit.Framework;

namespace SiliconStudio.ActionStack.Tests
{
    [TestFixture]
    class TestActionItem
    {
        [Test]
        public void TestConstruction()
        {
            var action = new SimpleActionItem();
            Assert.AreEqual(true, action.IsDone);
            Assert.AreEqual(false, action.IsFrozen);
            Assert.AreEqual(false, action.IsSaved);
        }

        [Test]
        public void TestUndo()
        {
            var action = new SimpleActionItem();
            action.Undo();
            Assert.AreEqual(false, action.IsDone);
            Assert.AreEqual(false, action.IsFrozen);
            Assert.AreEqual(false, action.IsSaved);
        }

        [Test]
        public void TestRedo()
        {
            var action = new SimpleActionItem();
            action.Redo();
            Assert.AreEqual(true, action.IsDone);
            Assert.AreEqual(false, action.IsFrozen);
            Assert.AreEqual(false, action.IsSaved);
        }

        [Test]
        public void TestFreeze()
        {
            var action = new SimpleActionItem();
            action.Freeze();
            Assert.AreEqual(true, action.IsDone);
            Assert.AreEqual(true, action.IsFrozen);
            Assert.AreEqual(false, action.IsSaved);
        }
    }
}
