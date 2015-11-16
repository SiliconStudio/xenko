using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SiliconStudio.ActionStack.Tests
{
    class ActionStackTestContainer
    {
        public ActionStackTestContainer(int capacity)
        {
            Stack = new ActionStack(capacity);
            Stack.ActionItemsAdded += (sender, e) => ActionItemsAdded.Add(Tuple.Create(sender, e));
            Stack.ActionItemsCleared += (sender, e) => ActionItemsCleared.Add(Tuple.Create(sender, e));
            Stack.ActionItemsDiscarded += (sender, e) => ActionItemsDiscarded.Add(Tuple.Create(sender, e));
            Stack.Undone += (sender, e) => Undone.Add(Tuple.Create(sender, e));
            Stack.Redone += (sender, e) => Redone.Add(Tuple.Create(sender, e));
        }

        public List<Tuple<object, ActionItemsEventArgs<IActionItem>>> ActionItemsAdded = new List<Tuple<object, ActionItemsEventArgs<IActionItem>>>();

        public List<Tuple<object, EventArgs>> ActionItemsCleared = new List<Tuple<object, EventArgs>>();

        public List<Tuple<object, DiscardedActionItemsEventArgs<IActionItem>>> ActionItemsDiscarded = new List<Tuple<object, DiscardedActionItemsEventArgs<IActionItem>>>();

        public List<Tuple<object, ActionItemsEventArgs<IActionItem>>> Undone = new List<Tuple<object, ActionItemsEventArgs<IActionItem>>>();

        public List<Tuple<object, ActionItemsEventArgs<IActionItem>>> Redone = new List<Tuple<object, ActionItemsEventArgs<IActionItem>>>();

        public ActionStack Stack;

        public void CheckRaiseCount(int added, int cleared, int discarded, int undone, int redone)
        {
            Assert.AreEqual(added, ActionItemsAdded.Count);
            Assert.AreEqual(cleared, ActionItemsCleared.Count);
            Assert.AreEqual(discarded, ActionItemsDiscarded.Count);
            Assert.AreEqual(undone, Undone.Count);
            Assert.AreEqual(redone, Redone.Count);
        }
    }
}