// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.ActionStack;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public class ValueChangedActionItem : DirtiableActionItem
    {
        protected ModelNodePath NodePath;
        protected object Index;
        protected object PreviousValue;
        private readonly ObservableViewModelIdentifier identifier;
        private ObservableViewModelService service;


        public ValueChangedActionItem(string name, ObservableViewModelService service, ModelNodePath nodePath, string observableNodePath, ObservableViewModelIdentifier identifier, object index, IEnumerable<IDirtiable> dirtiables, object previousValue)
            : base(name, dirtiables)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (!nodePath.IsValid) throw new InvalidOperationException("Unable to retrieve the path of the modified node.");
            this.service = service;
            this.identifier = identifier;
            PreviousValue = previousValue;
            NodePath = nodePath;
            Index = index;
            ObservableNodePath = observableNodePath;
        }

        public string ObservableNodePath { get; private set; }

        protected override void FreezeMembers()
        {
            service = null;
            NodePath = null;
            ObservableNodePath = null;
            Index = null;
            PreviousValue = null;
        }

        /// <inheritdoc/>
        protected override void UndoAction()
        {
            object pathIndex;
            var node = NodePath.GetSourceNode(out pathIndex);
            if (node == null)
                throw new InvalidOperationException("Unable to retrieve the node to modify in this undo process.");

            var currentValue = node.GetValue(Index);
            bool setByObservableNode = false;

            var observableNode = service.ResolveObservableNode(identifier, ObservableNodePath) as SingleObservableNode;
            if (observableNode != null)
            {
                observableNode.Value = PreviousValue;
                setByObservableNode = true;
            }

            if (!setByObservableNode)
            {
                node.SetValue(PreviousValue, Index);
            }

            PreviousValue = currentValue;
        }        

        /// <inheritdoc/>
        protected override void RedoAction()
        {
            // Once we have un-done, the previous value is updated so Redo is just Undoing the Undo
            UndoAction();
        }
    }
}
