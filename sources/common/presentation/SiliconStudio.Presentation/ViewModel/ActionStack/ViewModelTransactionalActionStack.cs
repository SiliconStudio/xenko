// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.ActionStack;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.ViewModel.ActionStack
{
    public class ViewModelTransactionalActionStack : TransactionalActionStack
    {
        private readonly List<DirtiableActionItem> discardedActionItems = new List<DirtiableActionItem>();

        public ViewModelTransactionalActionStack(int capacity, IViewModelServiceProvider serviceProvider)
            : base(capacity)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            ServiceProvider = serviceProvider;
            Dispatcher = serviceProvider.Get<IDispatcherService>();
        }

        public ViewModelTransactionalActionStack(int capacity, IViewModelServiceProvider serviceProvider, IEnumerable<IActionItem> initialActionsItems)
            : base(capacity, initialActionsItems)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            ServiceProvider = serviceProvider;
            Dispatcher = serviceProvider.Get<IDispatcherService>();
        }

        public IViewModelServiceProvider ServiceProvider { get; private set; }

        public IDispatcherService Dispatcher { get; private set; }
        
        public override void Add(IActionItem item)
        {
            Dispatcher.Invoke(() => base.Add(item));
        }

        public override SavePoint CreateSavePoint(bool markActionsAsSaved)
        {
            var savePoint = base.CreateSavePoint(markActionsAsSaved);
            if (markActionsAsSaved)
            {
                var dirtiables = new HashSet<IDirtiable>();
                foreach (var viewModelActionItem in EnumerateViewModelActionItems(discardedActionItems))
                {
                    var viewModelActionItemCopy = viewModelActionItem;
                    viewModelActionItem.Dirtiables.ForEach(dirtiable =>
                    {
                        dirtiable.DiscardActionItem(viewModelActionItemCopy);
                        dirtiables.Add(dirtiable);
                    });
                }
                foreach (var dirtiable in dirtiables)
                {
                    dirtiable.NotifyActionStackChange(ActionStackChange.Discarded);
                }
                discardedActionItems.Clear();
            }
            return savePoint;
        }

        protected override void OnActionItemsAdded(ActionItemsEventArgs<IActionItem> e)
        {
            RegisterActionItemsRecursively(e.ActionItems);
            base.OnActionItemsAdded(e);
        }

        protected override void OnActionItemsDiscarded(DiscardedActionItemsEventArgs<IActionItem> e)
        {
            DiscardActionItemsRecursively(e.ActionItems, e.Type);
            base.OnActionItemsDiscarded(e);
        }

        private static IEnumerable<DirtiableActionItem> EnumerateViewModelActionItems(IEnumerable<IActionItem> actionItems)
        {
            foreach (var actionItem in actionItems)
            {
                var viewModelActionItem = actionItem as DirtiableActionItem;
                if (viewModelActionItem != null)
                {
                    yield return viewModelActionItem;
                }
                var aggregateActionItem = actionItem as IAggregateActionItem;
                if (aggregateActionItem != null)
                {
                    foreach (var viewModelActionItem2 in EnumerateViewModelActionItems(aggregateActionItem.ActionItems))
                    {
                        yield return viewModelActionItem2;
                    }
                }
            }
        }

        private static void RegisterActionItemsRecursively(IEnumerable<IActionItem> actionItems)
        {
            var dirtiables = new HashSet<IDirtiable>();
            foreach (var viewModelActionItem in EnumerateViewModelActionItems(actionItems))
            {
                var viewModelActionItemCopy = viewModelActionItem;
                viewModelActionItem.Dirtiables.ForEach(dirtiable =>
                {
                    dirtiable.RegisterActionItem(viewModelActionItemCopy);
                    dirtiables.Add(dirtiable);
                });
            }

            foreach (var dirtiable in dirtiables)
            {
                dirtiable.NotifyActionStackChange(ActionStackChange.Added);
            }
        }

        private void DiscardActionItemsRecursively(IEnumerable<IActionItem> actionItems, ActionItemDiscardType discardType)
        {
            var dirtiables = new HashSet<IDirtiable>();
            foreach (var viewModelActionItem in EnumerateViewModelActionItems(actionItems))
            {
                if (discardType == ActionItemDiscardType.Swallowed)
                {
                    discardedActionItems.Add(viewModelActionItem);
                }
                else if (discardType != ActionItemDiscardType.UndoRedoInProgress)
                {
                    var viewModelActionItemCopy = viewModelActionItem;
                    viewModelActionItem.Dirtiables.ForEach(dirtiable =>
                    {
                        dirtiable.DiscardActionItem(viewModelActionItemCopy);
                        dirtiables.Add(dirtiable);
                    });
                }
            }

            foreach (var dirtiable in dirtiables)
            {
                dirtiable.NotifyActionStackChange(ActionStackChange.Discarded);
            }
        }
    }
}
