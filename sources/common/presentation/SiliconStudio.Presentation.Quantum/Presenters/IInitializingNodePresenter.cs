using System;
using System.Collections.Generic;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public interface IInitializingNodePresenter : INodePresenter
    {
        void AddChild(IInitializingNodePresenter child);

        void FinalizeInitialization();
    }
}
