using System;
using System.Collections.Generic;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public interface INodePresenterFactoryInternal
    {
        IReadOnlyCollection<INodePresenterCommand> AvailableCommands { get; }

        void CreateChildren(IInitializingNodePresenter parentPresenter, IObjectNode objectNode);
    }
}
