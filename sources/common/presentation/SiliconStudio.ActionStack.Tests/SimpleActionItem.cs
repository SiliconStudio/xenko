using System;

namespace SiliconStudio.ActionStack.Tests
{
    public class SimpleActionItem : ActionItem
    {
        private static int counter;
        private bool done = true;

        public SimpleActionItem()
        {
            Counter = ++counter;
        }

        public int Counter { get; }

        protected override void FreezeMembers()
        {
            Console.WriteLine($"Freezed {this}");
        }

        protected override void RedoAction()
        {
            Console.WriteLine($"Redoing {this}");
            done = true;
            Console.WriteLine($"Redone {this}");
        }

        protected override void UndoAction()
        {
            Console.WriteLine($"Undoing {this}");
            done = false;
            Console.WriteLine($"Undone {this}");
        }

        public override string ToString()
        {
            return $"ActionItem {Counter} (currently {(done ? "done" : "undone")})";
        }
    }
}