using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Transactions;
using SiliconStudio.Presentation.Dirtiables;

namespace SiliconStudio.Presentation.Tests.Dirtiables
{
    public class SimpleDirtyingOperation : Operation, IDirtyingOperation
    {
        private static int counter;

        public SimpleDirtyingOperation(IEnumerable<IDirtiable> dirtiables)
        {
            Counter = ++counter;
            Dirtiables = dirtiables.ToList();
        }

        public int Counter { get; }

        public bool IsDone { get; private set; } = true;

        public IReadOnlyList<IDirtiable> Dirtiables { get; }

        public Action OnUndo { get; set; }

        public Action OnRedo { get; set; }

        protected override void FreezeContent()
        {
            Console.WriteLine($"Freezed {this}");
        }

        protected override void Rollback()
        {
            Console.WriteLine($"Rollbacking {this}");
            IsDone = false;
            OnUndo?.Invoke();
            Console.WriteLine($"Rollbacked {this}");
        }

        protected override void Rollforward()
        {
            Console.WriteLine($"Rollforwarding {this}");
            IsDone = true;
            OnRedo?.Invoke();
            Console.WriteLine($"Rollforwarded {this}");
        }

        public override string ToString()
        {
            return $"Operation {Counter} (currently {(IsDone ? "done" : "undone")})";
        }
    }
}
