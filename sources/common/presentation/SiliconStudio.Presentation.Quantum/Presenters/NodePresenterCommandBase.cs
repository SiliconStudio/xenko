using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public abstract class NodePresenterCommandBase : INodePresenterCommand
    {
        public abstract string Name { get; }

        public virtual CombineMode CombineMode => CombineMode.CombineOnlyForAll;

        public abstract bool CanAttach(INodePresenter nodePresenter);

        public virtual Task<object> PreExecute(IEnumerable<INodePresenter> nodePresenters)
        {
            return Task.FromResult<object>(null);
        }

        public abstract Task Execute(INodePresenter nodePresenter, object parameter, object preExecuteResult);

        public virtual Task PostExecute(IEnumerable<INodePresenter> nodePresenters)
        {
            return Task.CompletedTask;
        }
    }
}