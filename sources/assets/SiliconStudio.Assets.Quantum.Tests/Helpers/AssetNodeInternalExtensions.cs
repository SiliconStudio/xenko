// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Assets.Quantum.Internal;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Tests.Helpers
{
    public static class AssetNodeInternalExtensions
    {
        public static OverrideType GetItemOverride(this IAssetNode node, Index index)
        {
            return ((IAssetObjectNodeInternal)node).GetItemOverride(index);
        }

        public static OverrideType GetKeyOverride(this IAssetNode node, Index index)
        {
            return ((IAssetObjectNodeInternal)node).GetKeyOverride(index);
        }
    }
}
