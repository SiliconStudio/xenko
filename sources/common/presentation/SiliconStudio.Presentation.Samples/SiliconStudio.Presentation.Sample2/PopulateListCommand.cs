using System;
using System.Collections.Generic;
using SiliconStudio.ActionStack;
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.ViewModel;

// This is a sample command that simply populate a list by adding 10 items into it.
// It supports undo/redo.

namespace SiliconStudio.Presentation.Sample2
{
    public class PopulateListCommand : CancellableCommand
    {
        public PopulateListCommand(IActionStack actionStack, IEnumerable<IDirtiableViewModel> dirtiables)
            : base(actionStack, dirtiables)
        {
        }

        public override string Name { get { return "PopulateList"; } }

        protected override UndoToken Redo(object parameter, bool creatingActionItem)
        {
            // Add 10 items and keep track of them in an undo token
            var addedItems = new string[10];
            var list = (IList<string>)parameter;
            for (int i = 0; i < 10; ++i)
            {
                var item = string.Format("Item {0}", Guid.NewGuid());
                list.Add(item);
                addedItems[i] = item;
            }
            return new UndoToken(true, addedItems);
        }

        protected override void Undo(object parameter, UndoToken token)
        {
            // Remove the items contained in the undo token
            var list = (IList<string>)parameter;
            var addedItems = (string[])token.TokenValue;
            foreach (var item in addedItems)
            {
                list.Remove(item);
            }
        }
    }
}