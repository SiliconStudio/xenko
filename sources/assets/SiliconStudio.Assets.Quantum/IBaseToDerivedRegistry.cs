// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Assets.Quantum
{
    public interface IBaseToDerivedRegistry
    {
        void RegisterBaseToDerived([CanBeNull] IAssetNode baseNode, [NotNull] IAssetNode derivedNode);

        [CanBeNull]
        IIdentifiable ResolveFromBase([CanBeNull] object baseObjectReference, [NotNull] IAssetNode derivedReferencerNode);
    }
}
