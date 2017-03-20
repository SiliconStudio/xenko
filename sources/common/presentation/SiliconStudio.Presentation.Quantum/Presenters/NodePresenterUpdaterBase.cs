using System;
using System.Collections.Generic;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public abstract class NodePresenterUpdaterBase : INodePresenterUpdater
    {
        public abstract void UpdateNode(INodePresenter node);
    }
}