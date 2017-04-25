// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Presentation.Quantum.Presenters;

namespace SiliconStudio.Presentation.Quantum.ViewModels
{
    public interface INodeViewModelFactory
    {
        NodeViewModel CreateGraph([NotNull] GraphViewModel owner, [NotNull] Type rootType, [NotNull] IEnumerable<INodePresenter> rootNodes);

        void GenerateChildren(GraphViewModel owner, NodeViewModel parent, List<INodePresenter> nodePresenters);
    }
}
