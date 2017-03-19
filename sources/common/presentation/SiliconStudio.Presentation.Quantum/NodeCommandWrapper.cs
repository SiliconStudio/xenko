using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Quantum.Presenters;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public class NodeCommandWrapper : NodeCommandWrapperBase
    {
        private readonly INodePresenter presenter;

        public NodeCommandWrapper(IViewModelServiceProvider serviceProvider, [NotNull] INodePresenter presenter, [NotNull] INodePresenterCommand command)
            : base(serviceProvider)
        {
            if (presenter == null) throw new ArgumentNullException(nameof(presenter));
            if (command == null) throw new ArgumentNullException(nameof(command));
            this.presenter = presenter;
            Command = command;
        }

        public override string Name => Command.Name;

        public override CombineMode CombineMode => Command.CombineMode;
        
        public INodePresenterCommand Command { get; }

        public override async Task Invoke(object parameter)
        {
            using (var transaction = UndoRedoService?.CreateTransaction())
            {
                var preExecuteResult = await Command.PreExecute(presenter.Yield(), parameter);
                await Command.Execute(presenter, parameter, preExecuteResult);
                await Command.PostExecute(presenter.Yield(), parameter);

                if (transaction != null)
                {
                    UndoRedoService?.SetName(transaction, ActionName);
                }
            }
        }
    }
}