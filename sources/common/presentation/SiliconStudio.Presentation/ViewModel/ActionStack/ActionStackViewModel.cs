// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;

using SiliconStudio.ActionStack;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Collections;
using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.ViewModel.ActionStack
{
    /// <summary>
    /// This class is a view model for the <see cref="ViewModelTransactionalActionStack"/> class. It inherits from the <see cref="DispatcherViewModel"/> class.
    /// </summary>
    public class ActionStackViewModel : DispatcherViewModel, IDisposable
    {
        private readonly ViewModelTransactionalActionStack actionStack;
        private readonly ObservableList<ActionItemViewModel> actionItems = new ObservableList<ActionItemViewModel>();
        private SavePoint savePoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionStackViewModel"/>.
        /// </summary>
        /// <param name="actionStack">The action stack. Cannot be null.</param>
        public ActionStackViewModel(ViewModelTransactionalActionStack actionStack)
            : base(actionStack.SafeArgument("actionStack").ServiceProvider)
        {
            this.actionStack = actionStack;

            actionStack.ActionItemsAdded += ActionItemsAdded;
            actionStack.ActionItemsCleared += ActionItemsCleared;
            actionStack.ActionItemsDiscarded += ActionItemsDiscarded;
            actionStack.Undone += ActionItemModified;
            actionStack.Redone += ActionItemModified;
        }

        /// <summary>
        /// Gets the action stack linked to this view model.
        /// </summary>
        public ViewModelTransactionalActionStack ActionStack { get { return actionStack; } }

        /// <summary>
        /// Gets the collection of action item view models currently contained in this view model.
        /// </summary>
        public IReadOnlyObservableCollection<ActionItemViewModel> ActionItems { get { return actionItems; } }

        /// <summary>
        /// Gets whether it is currently possible to perform an undo operation.
        /// </summary>
        public bool CanUndo { get { return ActionItems.Count > 0 && ActionItems.First().IsDone; } }

        /// <summary>
        /// Gets whether it is currently possible to perform a redo operation.
        /// </summary>
        public bool CanRedo { get { return ActionItems.Count > 0 && !ActionItems.Last().IsDone; } }

        /// <inheritdoc/>
        public void Dispose()
        {
            actionStack.ActionItemsAdded -= ActionItemsAdded;
            actionStack.ActionItemsCleared -= ActionItemsCleared;
            actionStack.ActionItemsDiscarded -= ActionItemsDiscarded;
            Dispatcher.Invoke(actionItems.Clear);
        }
        
        /// <summary>
        /// Notify that everything has been saved and create a save point in the action stack.
        /// </summary>
        public void NotifySave()
        {
            savePoint = actionStack.CreateSavePoint(true);
            actionItems.ForEach(x => x.IsSavePoint = x.ActionItem.Identifier == savePoint.ActionItemIdentifier);
        }

        private void ActionItemModified(object sender, ActionItemsEventArgs<IActionItem> e)
        {
            foreach (var actionItem in ActionItems)
                actionItem.Refresh();
        }

        private void ActionItemsDiscarded(object sender, DiscardedActionItemsEventArgs<IActionItem> e)
        {
            var actionsToRemove = e.ActionItems.Select(x => actionItems.FirstOrDefault(y => y.ActionItem.Identifier == x.Identifier)).Where(y => y != null).ToList();
            actionsToRemove.ForEach(x => actionItems.Remove(x));
        }

        private void ActionItemsCleared(object sender, EventArgs e)
        {
            actionStack.Clear();
        }

        private void ActionItemsAdded(object sender, ActionItemsEventArgs<IActionItem> e)
        {
            foreach (IActionItem actionItem in e.ActionItems)
                actionItems.Add(new ActionItemViewModel(actionStack.ServiceProvider, actionItem));
        }
    }
}
