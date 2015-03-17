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
        public readonly ModelNodePath NodePath;
        protected readonly ModelContainer ModelContainer;
        protected readonly ObservableViewModelService Service;
        protected readonly ObservableViewModelIdentifier Identifier;

        public override string Name { get { return NodeCommand.Name; } }

        public override CombineMode CombineMode { get { return NodeCommand.CombineMode; } }

        public ModelNodeCommandWrapper(IViewModelServiceProvider serviceProvider, INodeCommand nodeCommand, string observableNodePath, ObservableViewModel owner, ModelNodePath nodePath, IEnumerable<IDirtiableViewModel> dirtiables)
            : base(serviceProvider, dirtiables)
        {
            if (nodeCommand == null) throw new ArgumentNullException("nodeCommand");
            if (owner == null) throw new ArgumentNullException("owner");
            NodePath = nodePath;
            // Note: the owner should not be stored in the command because we want it to be garbage collectable
            Identifier = owner.Identifier;
            ModelContainer = owner.ModelContainer;
            NodeCommand = nodeCommand;
            Service = serviceProvider.Get<ObservableViewModelService>();
            ObservableNodePath = observableNodePath;
        }

        public INodeCommand NodeCommand { get; private set; }

        protected override UndoToken Redo(object parameter, bool creatingActionItem)
        {
            UndoToken token;
            object index;
            var modelNode = NodePath.GetSourceNode(out index);
            if (modelNode == null)
                throw new InvalidOperationException("Unable to retrieve the node on which to apply the redo operation.");

            var newValue = NodeCommand.Invoke(modelNode.Content.Value, modelNode.Content.Descriptor, parameter, out token);
            modelNode.SetValue(newValue, index);
            Refresh(modelNode, index);
            return token;
        }

        protected override void Undo(object parameter, UndoToken token)
        {
            object index;
            var modelNode = NodePath.GetSourceNode(out index);
            if (modelNode == null)
                throw new InvalidOperationException("Unable to retrieve the node on which to apply the undo operation.");

            var newValue = NodeCommand.Undo(modelNode.Content.Value, modelNode.Content.Descriptor, token);
            modelNode.SetValue(newValue, index);
            Refresh(modelNode, index);
        }

        /// <summary>
        /// Refreshes the <see cref="ObservableNode"/> corresponding to the given <see cref="IModelNode"/>, if an <see cref="ObservableViewModel"/>
        /// is available in the current.<see cref="IViewModelServiceProvider"/>.
        /// </summary>
        /// <param name="modelNode">The model node to use to fetch a corresponding <see cref="ObservableNode"/>.</param>
        /// <param name="index">The index at which the actual value to update is stored.</param>
        protected virtual void Refresh(IModelNode modelNode, object index)
        {
            if (modelNode == null) throw new ArgumentNullException("modelNode");
            var observableViewModel = Service.ViewModelProvider(Identifier);

            // No view model to refresh
            if (observableViewModel == null)
                return;

            var observableNode = (ObservableModelNode)observableViewModel.ResolveObservableNode(ObservableNodePath);
            // No node matches this model node
            if (observableNode == null)
                return;

            var newValue = modelNode.GetValue(index);

            if (observableNode.IsPrimitive)
            {
                var collectionDescriptor = modelNode.Content.Descriptor as CollectionDescriptor;
                if (collectionDescriptor != null)
                    newValue = collectionDescriptor.GetValue(modelNode.Content.Value, observableNode.Index);

                var dictionaryDescriptor = modelNode.Content.Descriptor as DictionaryDescriptor;
                if (dictionaryDescriptor != null)
                    newValue = dictionaryDescriptor.GetValue(newValue, observableNode.Index);
            }

            observableNode.ForceSetValue(newValue);
            observableNode.Owner.NotifyNodeChanged(observableNode.Path);
        }
    }
}
