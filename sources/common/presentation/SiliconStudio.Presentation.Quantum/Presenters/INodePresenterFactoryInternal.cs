using System.Collections.Generic;
using SiliconStudio.Presentation.Quantum.Presenters;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public interface INodePresenterFactoryInternal
    {
        IReadOnlyCollection<INodePresenterCommand> AvailableCommands { get; }

        void CreateChildren(IInitializingNodePresenter parentPresenter, IObjectNode objectNode);
    }
}
