// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Presentation.ViewModel.ActionStack;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public class ValueChangedActionItem : ViewModelActionItem
    {
        private ObservableViewModelService service;

        private ModelNodePath nodePath;
        private string observableNodePath;
        private readonly ObservableViewModelIdentifier identifier;

        private object index;
        private object previousValue;

        public ValueChangedActionItem(string name, ObservableViewModelService service, ModelNodePath nodePath, string observableNodePath, ObservableViewModelIdentifier identifier, object index, IEnumerable<IDirtiableViewModel> dirtiables, object previousValue)
            : base(name, dirtiables)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (!nodePath.IsValid) throw new InvalidOperationException("Unable to retrieve the path of the modified node.");
            this.service = service;
            this.nodePath = nodePath;
            this.index = index;
            this.observableNodePath = observableNodePath;
            this.identifier = identifier;
            this.previousValue = previousValue;
        }

        public string ObservableNodePath { get { return observableNodePath; } }

        protected override void FreezeMembers()
        {
            service = null;
            nodePath = null;
            observableNodePath = null;
            index = null;
        }

        /// <inheritdoc/>
        protected override void UndoAction()
        {
            object pathIndex;
            var node = nodePath.GetSourceNode(out pathIndex);
            if (node == null)
                throw new InvalidOperationException("Unable to retrieve the node to modify in this undo process.");

            var currentValue = node.GetValue(index);
            bool setByObservableNode = false;

            var observableViewModel = service.ViewModelProvider != null ? service.ViewModelProvider(identifier) : null;
            if (observableViewModel != null)
            {
                var observableNode = (SingleObservableNode)observableViewModel.ResolveObservableNode(observableNodePath);
                if (observableNode != null)
                {
                    observableNode.Value = previousValue;
                    setByObservableNode = true;
                }
            }

            if (!setByObservableNode)
            {
                node.SetValue(previousValue, index);
            }
            
            previousValue = currentValue;        
        }

        /// <inheritdoc/>
        protected override void RedoAction()
        {
            // Once we have un-done, the previous value is updated so Redo is just Undoing the Undo
            UndoAction();
        }
    }
}
