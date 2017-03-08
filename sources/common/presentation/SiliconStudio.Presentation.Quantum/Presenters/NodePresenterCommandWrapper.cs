using System.Threading.Tasks;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public class NodePresenterCommandWrapper : NodeCommandWrapperBase
    {
        private readonly NodePresenterBase presenter;
        private readonly INodeCommand command;

        public NodePresenterCommandWrapper(IViewModelServiceProvider serviceProvider, NodePresenterBase presenter, INodeCommand command)
            : base(serviceProvider)
        {
            this.presenter = presenter;
            this.command = command;
        }

        public override async Task Invoke(object parameter)
        {
            using (var transaction = UndoRedoService?.CreateTransaction())
            {
                await presenter.RunCommand(command, parameter);
                if (transaction != null)
                {
                    UndoRedoService?.SetName(transaction, ActionName);
                }
            }
        }

        public override string Name => command.Name;

        public override CombineMode CombineMode => command.CombineMode;
    }
}
