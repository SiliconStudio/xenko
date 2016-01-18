// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.ActionStack;
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
            public readonly object Parameter;

            public TokenData(TToken token, object parameter)
            {
                Token = token;
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
        
        public INodeCommand NodeCommand { get; }

        protected override async Task<UndoToken> InvokeInternal(object parameter)
        {
            object index;
            var modelNode = NodePath.GetSourceNode(out index);
            if (modelNode == null)
                throw new InvalidOperationException("Unable to retrieve the node on which to apply the redo operation.");

            var actionItem = await NodeCommand.Execute2(modelNode.Content, index, parameter, Dirtiables);
            if (actionItem != null)
            {
                ServiceProvider.Get<ITransactionalActionStack>().Add(actionItem);
            }
            return new UndoToken(false);
        }
    }
}
