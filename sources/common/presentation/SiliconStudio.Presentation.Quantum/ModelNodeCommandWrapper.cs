// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.ActionStack;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Presentation.Quantum
{
    public class ModelNodeCommandWrapper : NodeCommandWrapperBase
    {
        protected readonly ModelNodePath NodePath;
        protected readonly ModelContainer ModelContainer;
        private readonly ObservableViewModelService service;
        private readonly ObservableViewModelIdentifier identifier;

        public override string Name { get { return NodeCommand.Name; } }

        public override CombineMode CombineMode { get { return NodeCommand.CombineMode; } }

        public ModelNodeCommandWrapper(IViewModelServiceProvider serviceProvider, INodeCommand nodeCommand, string observableNodePath, ObservableViewModel owner, ModelNodePath nodePath, IEnumerable<IDirtiableViewModel> dirtiables)
            : base(serviceProvider, dirtiables)
        {
            if (nodeCommand == null) throw new ArgumentNullException("nodeCommand");
            if (owner == null) throw new ArgumentNullException("owner");
            NodePath = nodePath;
            // Note: the owner should not be stored in the command because we want it to be garbage collectable
            identifier = owner.Identifier;
            ModelContainer = owner.ModelContainer;
            NodeCommand = nodeCommand;
            service = serviceProvider.Get<ObservableViewModelService>();
            ObservableNodePath = observableNodePath;
        }

        public INodeCommand NodeCommand { get; private set; }
        
        internal IModelNode GetCommandRootNode()
        {
            return NodePath.RootNode;
        }

        protected override UndoToken Redo(object parameter, bool creatingActionItem)
        {
            UndoToken token;
            var modelNode = NodePath.GetNode();
            if (modelNode == null)
                throw new InvalidOperationException("Unable to retrieve the node on which to apply the redo operation.");

            var newValue = NodeCommand.Invoke(modelNode.Content.Value, modelNode.Content.Descriptor, parameter, out token);
            Refresh(modelNode, newValue);
            return token;
        }

        protected override void Undo(object parameter, UndoToken token)
        {
            var modelNode = NodePath.GetNode();
            if (modelNode == null)
                throw new InvalidOperationException("Unable to retrieve the node on which to apply the redo operation.");

            var newValue = NodeCommand.Undo(modelNode.Content.Value, modelNode.Content.Descriptor, token);
            Refresh(modelNode, newValue);
        }

        protected virtual void Refresh(IModelNode modelNode, object newValue)
        {
            var observableViewModel = service.ViewModelProvider(identifier);

            if (modelNode == null) throw new ArgumentNullException("modelNode");
            var observableNode = observableViewModel != null ? observableViewModel.ResolveObservableModelNode(ObservableNodePath, NodePath.RootNode) : null;
            
            // If we have an observable node, we use it to set the new value so the UI can be notified at the same time.
            if (observableNode != null)
            {
                if (observableNode.IsPrimitive)
                {
                    var collectionDescriptor = modelNode.Content.Descriptor as CollectionDescriptor;
                    if (collectionDescriptor != null)
                        newValue = collectionDescriptor.GetValue(newValue, observableNode.Index);

                    var dictionaryDescriptor = modelNode.Content.Descriptor as DictionaryDescriptor;
                    if (dictionaryDescriptor != null)
                        newValue = dictionaryDescriptor.GetValue(newValue, observableNode.Index);
                }

                observableNode.Value = newValue;
                observableNode.Owner.NotifyNodeChanged(observableNode.Path);
            }
            else
            {
                modelNode.Content.Value = newValue;
                ModelContainer.UpdateReferences(modelNode);
            }
        }
    }
}
