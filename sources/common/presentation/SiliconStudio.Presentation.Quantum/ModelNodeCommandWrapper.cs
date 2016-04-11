// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Presentation.Quantum
{
    public class ModelNodeCommandWrapper : NodeCommandWrapperBase
    {
        public readonly GraphNodePath NodePath;
        protected readonly ObservableViewModelService Service;

        public ModelNodeCommandWrapper(IViewModelServiceProvider serviceProvider, INodeCommand nodeCommand, GraphNodePath nodePath)
            : base(serviceProvider)
        {
            if (nodeCommand == null) throw new ArgumentNullException(nameof(nodeCommand));
            NodePath = nodePath;
            NodeCommand = nodeCommand;
            Service = serviceProvider.Get<ObservableViewModelService>();
        }

        public override string Name => NodeCommand.Name;

        public override CombineMode CombineMode => NodeCommand.CombineMode;
        
        public INodeCommand NodeCommand { get; }

        public override void Invoke(object parameter)
        {
            using (var transaction = ActionService.CreateTransaction())
            {
                object index;
                var modelNode = NodePath.GetSourceNode(out index);
                if (modelNode == null)
                    throw new InvalidOperationException("Unable to retrieve the node on which to apply the redo operation.");

                NodeCommand.Execute(modelNode.Content, index, parameter);
                ActionService.SetName(transaction, ActionName);
            }
        }
    }
}
