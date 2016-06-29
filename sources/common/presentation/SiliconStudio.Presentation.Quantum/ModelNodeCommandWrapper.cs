// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Presentation.Quantum
{
    public class ModelNodeCommandWrapper : NodeCommandWrapperBase
    {
        public readonly GraphNodePath NodePath;
        public readonly Index Index;
        protected readonly ObservableViewModelService Service;

        public ModelNodeCommandWrapper(IViewModelServiceProvider serviceProvider, INodeCommand nodeCommand, GraphNodePath nodePath, Index index)
            : base(serviceProvider)
        {
            if (nodeCommand == null) throw new ArgumentNullException(nameof(nodeCommand));
            NodePath = nodePath;
            Index = index;
            NodeCommand = nodeCommand;
            Service = serviceProvider.Get<ObservableViewModelService>();
        }

        public override string Name => NodeCommand.Name;

        public override CombineMode CombineMode => NodeCommand.CombineMode;
        
        public INodeCommand NodeCommand { get; }

        public override async Task Invoke(object parameter)
        {
            using (var transaction = ActionService?.CreateTransaction())
            {
                var modelNode = NodePath.GetNode();
                if (modelNode == null)
                    throw new InvalidOperationException("Unable to retrieve the node on which to apply the redo operation.");

                await NodeCommand.Execute(modelNode.Content, Index, parameter);
                ActionService?.SetName(transaction, ActionName);
            }
        }
    }
}
