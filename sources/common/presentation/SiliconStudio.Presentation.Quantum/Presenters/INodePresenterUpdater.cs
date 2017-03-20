using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public interface INodePresenterUpdater
    {
        void UpdateNode([NotNull] INodePresenter node);
    }
}