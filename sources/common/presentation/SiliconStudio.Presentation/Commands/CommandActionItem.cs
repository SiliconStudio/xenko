// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.ActionStack;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Presentation.ViewModel.ActionStack;

namespace SiliconStudio.Presentation.Commands
{
    public class CommandActionItem : DirtiableActionItem
    {
        private CancellableCommand command;
        private object parameter;
        private UndoToken undoToken;

        public CommandActionItem(CancellableCommand command, object parameter, UndoToken undoToken, IEnumerable<IDirtiable> dirtiables)
            : base("Executing " + command.Name, dirtiables)
        {
            this.command = command;
            this.parameter = parameter;
            this.undoToken = undoToken;
        }

        public CancellableCommand Command { get { return command; } }

        protected override void FreezeMembers()
        {
            command = null;
            parameter = null;
            undoToken = default(UndoToken);
        }

        protected override void UndoAction()
        {
            command.UndoCommand(parameter, undoToken);
        }

        protected override void RedoAction()
        {
            command.ExecuteCommand(parameter, false);
        }
    }
}
