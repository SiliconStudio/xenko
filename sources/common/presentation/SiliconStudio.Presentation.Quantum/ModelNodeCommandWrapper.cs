// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.ActionStack;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Presentation.Quantum
{
    public class ModelNodeCommandWrapper : NodeCommandWrapperBase
    {
        private readonly INodeCommand nodeCommand;
        private readonly ObservableViewModelService service;
        private readonly ObservableViewModelIdentifier identifier;
        private readonly ModelNodePath nodePath;
        private readonly ModelContainer modelContainer;

        public override string Name { get { return nodeCommand.Name; } }

        public override CombineMode CombineMode { get { return nodeCommand.CombineMode; } }

        public ModelNodeCommandWrapper(IViewModelServiceProvider serviceProvider, INodeCommand nodeCommand, string observableNodePath, ObservableViewModelIdentifier identifier, ModelNodePath nodePath, ModelContainer modelContainer, IEnumerable<IDirtiableViewModel> dirtiables)
            : base(serviceProvider, dirtiables)
        {
            if (nodeCommand == null) throw new ArgumentNullException("nodeCommand");
            if (modelContainer == null) throw new ArgumentNullException("modelContainer");
            this.identifier = identifier;
            this.nodePath = nodePath;
            this.modelContainer = modelContainer;
            this.nodeCommand = nodeCommand;
            service = serviceProvider.Get<ObservableViewModelService>();
            ObservableNodePath = observableNodePath;
        }

        internal IModelNode GetCommandRootNode()
        {
            return nodePath.RootNode;
        }

        protected override UndoToken Redo(object parameter, bool creatingActionItem)
        {
            UndoToken token;
            var viewModelNode = nodePath.GetNode();
            if (viewModelNode == null)
                throw new InvalidOperationException("Unable to retrieve the node on which to apply the redo operation.");

            var newValue = nodeCommand.Invoke(viewModelNode.Content.Value, viewModelNode.Content.Descriptor, parameter, out token);
            Refresh(viewModelNode, newValue);
            return token;
        }

        protected override void Undo(object parameter, UndoToken token)
        {
            var viewModelNode = nodePath.GetNode();
            if (viewModelNode == null)
                throw new InvalidOperationException("Unable to retrieve the node on which to apply the redo operation.");

            var newValue = nodeCommand.Undo(viewModelNode.Content.Value, viewModelNode.Content.Descriptor, token);
            Refresh(viewModelNode, newValue);
        }

        private void Refresh(IModelNode modelNode, object newValue)
        {
            var observableViewModel = service.ViewModelProvider(identifier);

            if (modelNode == null) throw new ArgumentNullException("modelNode");
            var observableNode = observableViewModel != null ? observableViewModel.ResolveObservableModelNode(ObservableNodePath, nodePath.RootNode) : null;
            
            // If we have an observable node, we use it to set the new value so the UI can be notified at the same time.
            if (observableNode != null)
            {
                observableNode.Value = newValue;
                observableNode.Owner.NotifyNodeChanged(observableNode.Path);
            }
            else
            {
                modelNode.Content.Value = newValue;
                modelContainer.UpdateReferences(modelNode);
            }
        }
    }
}
