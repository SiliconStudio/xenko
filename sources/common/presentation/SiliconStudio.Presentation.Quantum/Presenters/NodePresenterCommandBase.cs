// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public abstract class NodePresenterCommandBase : INodePresenterCommand
    {
        public abstract string Name { get; }

        public virtual CombineMode CombineMode => CombineMode.CombineOnlyForAll;

        public abstract bool CanAttach(INodePresenter nodePresenter);

        public virtual bool CanExecute(IReadOnlyCollection<INodePresenter> nodePresenters, object parameter)
        {
            return true;
        }

        [NotNull]
        public virtual Task<object> PreExecute(IReadOnlyCollection<INodePresenter> nodePresenters, object parameter)
        {
            return Task.FromResult<object>(null);
        }

        public abstract Task Execute(INodePresenter nodePresenter, object parameter, object preExecuteResult);

        [NotNull]
        public virtual Task PostExecute(IReadOnlyCollection<INodePresenter> nodePresenters, object parameter)
        {
            return Task.CompletedTask;
        }
    }
}
