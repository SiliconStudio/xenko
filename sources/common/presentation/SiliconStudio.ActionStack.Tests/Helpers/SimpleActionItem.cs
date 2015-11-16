using System;

namespace SiliconStudio.ActionStack.Tests.Helpers
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

        public Action OnUndo { get; set; }

        public Action OnRedo { get; set; }

        protected override void FreezeMembers()
        {
            Console.WriteLine($"Freezed {this}");
        }

        protected override void RedoAction()
        {
            Console.WriteLine($"Redoing {this}");
            done = true;
            OnRedo?.Invoke();
            Console.WriteLine($"Redone {this}");
        }

        protected override void UndoAction()
        {
            Console.WriteLine($"Undoing {this}");
            done = false;
            OnUndo?.Invoke();
            Console.WriteLine($"Undone {this}");
        }

        public override string ToString()
        {
            return $"ActionItem {Counter} (currently {(done ? "done" : "undone")})";
        }
    }
}