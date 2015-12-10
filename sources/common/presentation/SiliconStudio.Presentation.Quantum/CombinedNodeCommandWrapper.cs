// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.ActionStack;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public class CombinedNodeCommandWrapper : NodeCommandWrapperBase
    {
        private readonly IReadOnlyCollection<ModelNodeCommandWrapper> commands;
        private readonly IViewModelServiceProvider serviceProvider;

        public CombinedNodeCommandWrapper(IViewModelServiceProvider serviceProvider, string name, IReadOnlyCollection<ModelNodeCommandWrapper> commands)
            : base(serviceProvider, new HashSet<IDirtiable>(commands.SafeArgument(nameof(commands)).SelectMany(x => x.Dirtiables)))
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (commands.Count == 0) throw new ArgumentException(@"The collection of commands to combine is empty", nameof(commands));
            if (commands.Any(x => !ReferenceEquals(x.NodeCommand, commands.First().NodeCommand))) throw new ArgumentException(@"The collection of commands to combine cannot contain different node commands", nameof(commands));
            this.commands = commands;
            Name = name;
            this.serviceProvider = serviceProvider;
        }

        public override string Name { get; }

        public override CombineMode CombineMode => CombineMode.DoNotCombine;

        private ITransactionalActionStack ActionStack => serviceProvider.Get<ITransactionalActionStack>();

        public override RedoToken Undo(UndoToken undoToken)
        {
            var undoTokens = (Dictionary<ModelNodeCommandWrapper, UndoToken>)undoToken.TokenValue;
            var redoTokens = new Dictionary<ModelNodeCommandWrapper, RedoToken>();
            foreach (var command in commands)
            {
                redoTokens[command] = command.Undo(undoTokens[command]);
            }
            return new RedoToken(redoTokens);
        }

        public override UndoToken Redo(RedoToken redoToken)
        {
            var redoTokens = (Dictionary<ModelNodeCommandWrapper, RedoToken>)redoToken.TokenValue;
            var undoTokens = new Dictionary<ModelNodeCommandWrapper, UndoToken>();
            bool canUndo = false;

            foreach (var command in commands)
            {
                var undoToken = command.Redo(redoTokens[command]);
                undoTokens.Add(command, undoToken);
                canUndo = canUndo || undoToken.CanUndo;
            }

            return new UndoToken(canUndo, undoTokens);
        }

        protected override UndoToken Do(object parameter)
        {
            ActionStack.BeginTransaction();
            var undoTokens = new Dictionary<ModelNodeCommandWrapper, UndoToken>();
            bool canUndo = false;

            commands.First().NodeCommand.StartCombinedInvoke();

            foreach (var command in commands)
            {
                var undoToken = command.Invoke(parameter);
                undoTokens.Add(command, undoToken);
                canUndo = canUndo || undoToken.CanUndo;
            }

            commands.First().NodeCommand.EndCombinedInvoke();

            var displayName = "Executing " + Name;

            ActionStack.EndTransaction(displayName, x => new AggregateActionItem(displayName, x.ToArray()));
            return new UndoToken(canUndo, undoTokens);
        }
    }
}
