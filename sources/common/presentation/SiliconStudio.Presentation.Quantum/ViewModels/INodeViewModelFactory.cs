using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Presentation.Quantum.Presenters;

namespace SiliconStudio.Presentation.Quantum
{
    public interface INodeViewModelFactory
    {
        NodeViewModel CreateGraph([NotNull] GraphViewModel owner, [NotNull] Type rootType, [NotNull] IEnumerable<INodePresenter> rootNodes);
    }
}