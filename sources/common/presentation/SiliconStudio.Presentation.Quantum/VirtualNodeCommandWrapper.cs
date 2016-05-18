// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Presentation.Quantum
{
    public class VirtualNodeCommandWrapper : NodeCommandWrapperBase
    {
        private readonly IGraphNode node;
        private readonly Index index;
        protected readonly ObservableViewModelService Service;

        public VirtualNodeCommandWrapper(IViewModelServiceProvider serviceProvider, INodeCommand nodeCommand, IGraphNode node, Index index)
            : base(serviceProvider)
        {
            if (nodeCommand == null) throw new ArgumentNullException(nameof(nodeCommand));
            if (node == null) throw new ArgumentNullException(nameof(node));
            this.node = node;
            this.index = index;
            NodeCommand = nodeCommand;
            Service = serviceProvider.Get<ObservableViewModelService>();
        }

        public override string Name => NodeCommand.Name;

        public override CombineMode CombineMode => NodeCommand.CombineMode;

        public INodeCommand NodeCommand { get; }

        public override async Task Invoke(object parameter)
        {
            using (var transaction = ActionService.CreateTransaction())
            {
                await NodeCommand.Execute(node.Content, index, parameter);
                ActionService.SetName(transaction, ActionName);
            }
        }
    }
}
