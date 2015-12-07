// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.ActionStack;
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Presentation.Quantum
{
    public class ModelNodeCommandWrapper : NodeCommandWrapperBase
    {
        private class ModelNodeToken
        {
            public readonly UndoToken Token;
            public readonly UndoToken AdditionalToken;

            public ModelNodeToken(UndoToken token, UndoToken additionalToken)
            {
                Token = token;
                AdditionalToken = additionalToken;
            }
        }

        public readonly GraphNodePath NodePath;
        protected readonly NodeContainer NodeContainer;
        protected readonly ObservableViewModelService Service;
        protected readonly ObservableViewModelIdentifier Identifier;

        public ModelNodeCommandWrapper(IViewModelServiceProvider serviceProvider, INodeCommand nodeCommand, string observableNodePath, ObservableViewModel owner, GraphNodePath nodePath, IEnumerable<IDirtiable> dirtiables)
            : base(serviceProvider, dirtiables)
        {
            if (nodeCommand == null) throw new ArgumentNullException(nameof(nodeCommand));
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            NodePath = nodePath;
            // Note: the owner should not be stored in the command because we want it to be garbage collectable
            Identifier = owner.Identifier;
            NodeContainer = owner.NodeContainer;
            NodeCommand = nodeCommand;
            Service = serviceProvider.Get<ObservableViewModelService>();
            ObservableNodePath = observableNodePath;
        }

        public override string Name => NodeCommand.Name;

        public override CombineMode CombineMode => NodeCommand.CombineMode;

        public virtual CancellableCommand AdditionalCommand { get; set; }
        
        public INodeCommand NodeCommand { get; }

        protected override UndoToken Redo(object parameter, bool creatingActionItem)
        {
            UndoToken token;
            object index;
            var modelNode = NodePath.GetSourceNode(out index);
            if (modelNode == null)
                throw new InvalidOperationException("Unable to retrieve the node on which to apply the redo operation.");

            var currentValue = modelNode.Content.Retrieve(index);
            var newValue = NodeCommand.Invoke(currentValue, parameter, out token);
            modelNode.Content.Update(newValue, index);
            Refresh(modelNode, index);

            var additionalToken = new UndoToken();
            if (AdditionalCommand != null)
            {
                additionalToken = AdditionalCommand.ExecuteCommand(null, false);
            }
            return new UndoToken(token.CanUndo, new ModelNodeToken(token, additionalToken));
        }

        protected override void Undo(object parameter, UndoToken token)
        {
            object index;
            var modelNode = NodePath.GetSourceNode(out index);
            if (modelNode == null)
                throw new InvalidOperationException("Unable to retrieve the node on which to apply the undo operation.");

            var modelNodeToken = (ModelNodeToken)token.TokenValue;
            var currentValue = modelNode.Content.Retrieve(index);
            var newValue = NodeCommand.Undo(currentValue, modelNodeToken.Token);
            modelNode.Content.Update(newValue, index);
            Refresh(modelNode, index);

            AdditionalCommand?.UndoCommand(null, modelNodeToken.AdditionalToken);
        }

        /// <summary>
        /// Refreshes the <see cref="ObservableNode"/> corresponding to the given <see cref="IGraphNode"/>, if an <see cref="ObservableViewModel"/>
        /// is available in the current.<see cref="IViewModelServiceProvider"/>.
        /// </summary>
        /// <param name="modelNode">The model node to use to fetch a corresponding <see cref="ObservableNode"/>.</param>
        /// <param name="index">The index at which the actual value to update is stored.</param>
        protected virtual void Refresh(IGraphNode modelNode, object index)
        {
            if (modelNode == null) throw new ArgumentNullException(nameof(modelNode));

            var observableNode = Service.ResolveObservableNode(Identifier, ObservableNodePath) as ObservableModelNode;
            // No node matches this model node
            if (observableNode == null)
                return;

            var newValue = modelNode.Content.Retrieve(index);

            observableNode.ForceSetValue(newValue);
            observableNode.Owner.NotifyNodeChanged(observableNode.Path);
        }
    }
}
