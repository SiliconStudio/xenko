// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
