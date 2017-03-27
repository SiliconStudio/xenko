using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public interface INodePresenterFactory
    {      
        INodePresenter CreateNodeHierarchy([NotNull] IObjectNode rootNode, [NotNull] GraphNodePath rootNodePath, [CanBeNull] IPropertyProviderViewModel propertyProvider = null);

        bool IsPrimitiveType(Type type);
    }
}
