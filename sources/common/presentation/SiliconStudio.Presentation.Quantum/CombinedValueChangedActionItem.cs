// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.ActionStack;

namespace SiliconStudio.Presentation.Quantum
{
    public class CombinedValueChangedActionItem : AggregateActionItem
    {
        private readonly ObservableViewModelService serviceProvider;
        private readonly string observableNodePath;
        private readonly ObservableViewModelIdentifier identifier;

        internal CombinedValueChangedActionItem(string displayName, ObservableViewModelService serviceProvider, string observableNodePath, ObservableViewModelIdentifier identifier, IEnumerable<IActionItem> actionItems)
            : base(displayName, actionItems)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            this.serviceProvider = serviceProvider;
            this.observableNodePath = observableNodePath;
            this.identifier = identifier;
        }

        public string ObservableNodePath { get { return observableNodePath; } }

        /// <inheritdoc/>
        protected override void UndoAction()
        {
            base.UndoAction();
            Refresh();
        }
        /// <inheritdoc/>
        protected override void RedoAction()
        {
            base.RedoAction();
            Refresh();
        }

        private void Refresh()
        {
            var observableViewModel = serviceProvider.ViewModelProvider != null ? serviceProvider.ViewModelProvider(identifier) : null;
            if (observableViewModel != null)
            {
                var combinedNode = observableViewModel.ResolveObservableNode(observableNodePath) as CombinedObservableNode;
                if (combinedNode != null)
                {
                    combinedNode.Refresh();
                    combinedNode.Owner.NotifyNodeChanged(combinedNode.Path);
                }
            }
        }
    }
}