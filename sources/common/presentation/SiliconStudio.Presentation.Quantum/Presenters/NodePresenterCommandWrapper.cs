using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public class NodePresenterCommandWrapper : NodeCommandWrapperBase
    {
        private readonly IReadOnlyCollection<INodePresenter> presenters;

        public NodePresenterCommandWrapper(IViewModelServiceProvider serviceProvider, [NotNull] IReadOnlyCollection<INodePresenter> presenters, [NotNull] INodePresenterCommand command)
            : base(serviceProvider)
        {
            if (presenters == null) throw new ArgumentNullException(nameof(presenters));
            if (command == null) throw new ArgumentNullException(nameof(command));
            this.presenters = presenters;
            Command = command;
        }

        public override string Name => Command.Name;

        public override CombineMode CombineMode => Command.CombineMode;
        
        public INodePresenterCommand Command { get; }

        public override bool CanExecute(object parameter)
        {
            return Command.CanExecute(presenters, parameter);
        }

        public override async Task Invoke(object parameter)
        {
            using (var transaction = UndoRedoService?.CreateTransaction())
            {
                var preExecuteResult = await Command.PreExecute(presenters, parameter);
                foreach (var presenter in presenters)
                {
                    await Command.Execute(presenter, parameter, preExecuteResult);
                }
                await Command.PostExecute(presenters.ToList(), parameter);

                if (transaction != null)
                {
                    UndoRedoService?.SetName(transaction, ActionName);
                }
            }
        }
    }
}