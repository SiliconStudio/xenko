// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public sealed class CombinedNodeCommandWrapper : NodeCommandWrapperBase
    {
        private readonly IReadOnlyList<NodeCommandWrapperBase> commands;
        private readonly ModelNodeCommandWrapper firstCommand;

        public CombinedNodeCommandWrapper(IViewModelServiceProvider serviceProvider, string name, IReadOnlyList<NodeCommandWrapperBase> commands)
            : base(serviceProvider)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (commands.Count == 0) throw new ArgumentException(@"The collection of commands to combine is empty", nameof(commands));

            firstCommand = commands[0] as ModelNodeCommandWrapper;
            if (firstCommand != null && commands.OfType<ModelNodeCommandWrapper>().Any(x => !ReferenceEquals(x.NodeCommand, firstCommand.NodeCommand)))
                throw new ArgumentException(@"The collection of commands to combine cannot contain different node commands", nameof(commands));

            this.commands = commands;
            Name = name;
        }

        public override string Name { get; }

        public override CombineMode CombineMode => CombineMode.DoNotCombine;

        public override async Task Invoke(object parameter)
        {
            using (var transaction = UndoRedoService?.CreateTransaction())
            {
                firstCommand?.NodeCommand.StartCombinedInvoke();
                foreach (var command in commands)
                {
                    await command.Invoke(parameter);
                }
                firstCommand?.NodeCommand.EndCombinedInvoke();
                UndoRedoService?.SetName(transaction, ActionName);
            }
        }
    }
}
