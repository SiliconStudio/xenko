using System;
using System.Collections.Generic;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public abstract class NodePresenterUpdaterBase : INodePresenterUpdater
    {
        public virtual void UpdateNode(INodePresenter node)
        {
            // Do nothing by default
        }

        public virtual void FinalizeTree(INodePresenter root)
        {
            // Do nothing by default
        }
    }
}