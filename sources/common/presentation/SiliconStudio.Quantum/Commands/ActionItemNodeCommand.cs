// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.ActionStack;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.Commands
{
    public abstract class ActionItemNodeCommand : NodeCommandBase
    {
        public sealed override Task<IActionItem> Execute2(IContent content, object index, object parameter, IEnumerable<IDirtiable> dirtiables)
        {
            var actionItem = CreateActionItem(content, index, parameter, dirtiables);
            return actionItem?.Do() ?? false ? Task.FromResult<IActionItem>(actionItem) : null;
        }

        protected abstract NodeCommandActionItem CreateActionItem(IContent content, object index, object parameter, IEnumerable<IDirtiable> dirtiables);

        [Obsolete]
        protected object Do(object currentValue, object parameter, out UndoToken undoToken)
        {
            undoToken = new UndoToken(false);
            return null;
        }
    }
}