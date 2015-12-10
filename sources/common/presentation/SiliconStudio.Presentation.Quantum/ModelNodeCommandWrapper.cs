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
        private class TokenData<TToken>
        {
            public readonly TToken Token;
            public readonly TToken AdditionalToken;
            public readonly object Parameter;

            public TokenData(TToken token, TToken additionalToken, object parameter)
            {
                Token = token;
                AdditionalToken = additionalToken;
                Parameter = parameter;
            }
        }

        public readonly GraphNodePath NodePath;
        protected readonly ObservableViewModelService Service;

        public ModelNodeCommandWrapper(IViewModelServiceProvider serviceProvider, INodeCommand nodeCommand, GraphNodePath nodePath, IEnumerable<IDirtiable> dirtiables)
            : base(serviceProvider, dirtiables)
        {
            if (nodeCommand == null) throw new ArgumentNullException(nameof(nodeCommand));
            NodePath = nodePath;
            NodeCommand = nodeCommand;
            Service = serviceProvider.Get<ObservableViewModelService>();
        }

        public override string Name => NodeCommand.Name;

        public override CombineMode CombineMode => NodeCommand.CombineMode;

        public virtual CancellableCommandBase AdditionalCommand { get; set; }
        
        public INodeCommand NodeCommand { get; }

        public override RedoToken Undo(UndoToken undoToken)
        {
            object index;
            var modelNode = NodePath.GetSourceNode(out index);
            if (modelNode == null)
                throw new InvalidOperationException("Unable to retrieve the node on which to apply the undo operation.");

            var nodeToken = (TokenData<UndoToken>)undoToken.TokenValue;
            var currentValue = modelNode.Content.Retrieve(index);
            RedoToken redoToken;
            var newValue = NodeCommand.Undo(currentValue, nodeToken.Token, out redoToken);
            modelNode.Content.Update(newValue, index);

            var additionalToken = AdditionalCommand?.Undo(nodeToken.AdditionalToken) ?? default(RedoToken);
            return new RedoToken(new TokenData<RedoToken>(redoToken, additionalToken, nodeToken.Parameter));
        }

        public override UndoToken Redo(RedoToken redoToken)
        {
            var tokenData = (TokenData<RedoToken>)redoToken.TokenValue;
            UndoToken token;
            object index;
            var modelNode = NodePath.GetSourceNode(out index);
            if (modelNode == null)
                throw new InvalidOperationException("Unable to retrieve the node on which to apply the redo operation.");

            var currentValue = modelNode.Content.Retrieve(index);
            var newValue = NodeCommand.Redo(currentValue, tokenData.Token, out token);
            modelNode.Content.Update(newValue, index);

            var additionalToken = new UndoToken();
            if (AdditionalCommand != null)
            {
                additionalToken = AdditionalCommand.Redo(tokenData.AdditionalToken);
            }
            return new UndoToken(token.CanUndo, new TokenData<UndoToken>(token, additionalToken, tokenData.Parameter));
        }

        protected override UndoToken Do(object parameter)
        {
            UndoToken token;
            object index;
            var modelNode = NodePath.GetSourceNode(out index);
            if (modelNode == null)
                throw new InvalidOperationException("Unable to retrieve the node on which to apply the redo operation.");

            var currentValue = modelNode.Content.Retrieve(index);
            var newValue = NodeCommand.Execute(currentValue, parameter, out token);
            modelNode.Content.Update(newValue, index);

            var additionalToken = new UndoToken();
            if (AdditionalCommand != null)
            {
                additionalToken = AdditionalCommand.Invoke(null);
            }
            return new UndoToken(token.CanUndo, new TokenData<UndoToken>(token, additionalToken, parameter));
        }
    }
}
