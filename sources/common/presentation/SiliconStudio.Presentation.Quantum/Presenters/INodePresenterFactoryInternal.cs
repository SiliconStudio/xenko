using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public interface INodePresenterFactoryInternal
    {
        IReadOnlyCollection<INodePresenterCommand> AvailableCommands { get; }

        void CreateChildren([NotNull] IInitializingNodePresenter parentPresenter, [NotNull] IObjectNode objectNode, [CanBeNull] IPropertyProviderViewModel propertyProvider = null);
    }
}
