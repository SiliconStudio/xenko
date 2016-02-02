// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;

using SiliconStudio.ActionStack;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Collections;

namespace SiliconStudio.Presentation.ViewModel.ActionStack
{
    /// <summary>
    /// This class is a view model for the <see cref="ITransactionalActionStack"/> class. It inherits from the <see cref="DispatcherViewModel"/> class.
    /// </summary>
    public class ActionStackViewModel : DispatcherViewModel, IDisposable
    {
        private readonly ObservableList<ActionItemViewModel> actionItems = new ObservableList<ActionItemViewModel>();
        private SavePoint savePoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionStackViewModel"/>.
        /// </summary>
        /// <param name="serviceProvider">The service provider related to this view model</param>
        /// <param name="actionStack">The action stack. Cannot be null.</param>
        public ActionStackViewModel(IViewModelServiceProvider serviceProvider, ITransactionalActionStack actionStack)
            : base(serviceProvider)
        {
            ActionStack = actionStack;

            actionStack.ActionItemsAdded += ActionItemsAdded;
            actionStack.ActionItemsCleared += ActionItemsCleared;
            actionStack.ActionItemsDiscarded += ActionItemsDiscarded;
            actionStack.Undone += ActionItemModified;
            actionStack.Redone += ActionItemModified;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ActionStack.ActionItemsAdded -= ActionItemsAdded;
            ActionStack.ActionItemsCleared -= ActionItemsCleared;
            ActionStack.ActionItemsDiscarded -= ActionItemsDiscarded;
            ActionStack.Undone -= ActionItemModified;
            ActionStack.Redone -= ActionItemModified;
            Dispatcher.Invoke(actionItems.Clear);
        }

        /// <summary>
        /// Gets the action stack linked to this view model.
        /// </summary>
        public ITransactionalActionStack ActionStack { get; }

        /// <summary>
        /// Gets the collection of action item view models currently contained in this view model.
        /// </summary>
        public IReadOnlyObservableCollection<ActionItemViewModel> ActionItems => actionItems;

        /// <summary>
        /// Gets whether it is currently possible to perform an undo operation.
        /// </summary>
        public bool CanUndo => ActionItems.Count > 0 && ActionItems.First().IsDone;

        /// <summary>
        /// Gets whether it is currently possible to perform a redo operation.
        /// </summary>
        public bool CanRedo => ActionItems.Count > 0 && !ActionItems.Last().IsDone;

        /// <summary>
        /// Notify that everything has been saved and create a save point in the action stack.
        /// </summary>
        public void NotifySave()
        {
            savePoint = ActionStack.CreateSavePoint(true);
            actionItems.ForEach(x => x.IsSavePoint = x.ActionItem.Identifier == savePoint.ActionItemIdentifier);
            var dirtiableManager = ServiceProvider.TryGet<DirtiableManager>();
            dirtiableManager.NotifySave();
        }

        private void ActionItemModified(object sender, ActionItemsEventArgs<IActionItem> e)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (var actionItem in ActionItems)
                    actionItem.Refresh();
            });
        }

        private void ActionItemsDiscarded(object sender, DiscardedActionItemsEventArgs<IActionItem> e)
        {
            var actionsToRemove = e.ActionItems.Select(x => actionItems.FirstOrDefault(y => y.ActionItem.Identifier == x.Identifier)).ToList();
            Dispatcher.Invoke(() => actionsToRemove.ForEach(x => actionItems.Remove(x)));
        }

        private void ActionItemsCleared(object sender, EventArgs e)
        {
            actionItems.Clear();
        }

        private void ActionItemsAdded(object sender, ActionItemsEventArgs<IActionItem> e)
        {
            Dispatcher.Invoke(() => e.ActionItems.ForEach(x => actionItems.Add(new ActionItemViewModel(ServiceProvider, x))));
        }
    }
}
