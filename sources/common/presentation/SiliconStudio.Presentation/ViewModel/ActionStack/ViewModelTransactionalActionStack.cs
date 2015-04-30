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
        private readonly List<ViewModelActionItem> discardedActionItems = new List<ViewModelActionItem>();

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
                discardedActionItems.ForEach(x => x.Dirtiables.ForEach(y => y.DiscardActionItem(x)));
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

        private static void RegisterActionItemsRecursively(IEnumerable<IActionItem> actionItems)
        {
            foreach (var actionItem in actionItems)
            {
                var viewModelActionItem = actionItem as ViewModelActionItem;
                if (viewModelActionItem != null)
                {
                    viewModelActionItem.Dirtiables.ForEach(x => x.RegisterActionItem(viewModelActionItem));
                }
                var aggregateActionItem = actionItem as IAggregateActionItem;
                if (aggregateActionItem != null)
                {
                    RegisterActionItemsRecursively(aggregateActionItem.ActionItems);
                }
            }
        }

        private void DiscardActionItemsRecursively(IEnumerable<IActionItem> actionItems, ActionItemDiscardType discardType)
        {
            foreach (var actionItem in actionItems)
            {
                var viewModelActionItem = actionItem as ViewModelActionItem;
                if (viewModelActionItem != null)
                {
                    if (discardType == ActionItemDiscardType.Swallowed)
                    {
                        discardedActionItems.Add(viewModelActionItem);
                    }
                    else if (discardType != ActionItemDiscardType.UndoRedoInProgress)
                    {
                        viewModelActionItem.Dirtiables.ForEach(x => x.DiscardActionItem(viewModelActionItem));
                    }
                }
                var aggregateActionItem = actionItem as IAggregateActionItem;
                if (aggregateActionItem != null)
                {
                    DiscardActionItemsRecursively(aggregateActionItem.ActionItems, discardType);
                }
            }
        }
    }
}
