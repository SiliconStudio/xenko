using System;
using System.Collections.Generic;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public interface INodePresenterFactory
    {
        INodePresenter CreateNodeHierarchy(IObjectNode rootNode, GraphNodePath rootNodePath);
    }
}
