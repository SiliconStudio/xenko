// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.ActionStack;

namespace SiliconStudio.Presentation.Quantum
{
    public class CombinedValueChangedActionItem : AggregateActionItem
    {
        private readonly ObservableViewModelService serviceProvider;
        private readonly ObservableViewModelIdentifier identifier;

        internal CombinedValueChangedActionItem(string displayName, ObservableViewModelService serviceProvider, string observableNodePath, ObservableViewModelIdentifier identifier, IEnumerable<IActionItem> actionItems)
            : base(displayName, actionItems.ToArray())
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            this.serviceProvider = serviceProvider;
            this.identifier = identifier;
            ObservableNodePath = observableNodePath;
        }

        public string ObservableNodePath { get; }

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
            var combinedNode = serviceProvider.ResolveObservableNode(identifier, ObservableNodePath) as CombinedObservableNode;
            if (combinedNode != null)
            {
                combinedNode.Refresh();
                combinedNode.Owner.NotifyNodeChanged(combinedNode.Path);
            }
        }
    }
}