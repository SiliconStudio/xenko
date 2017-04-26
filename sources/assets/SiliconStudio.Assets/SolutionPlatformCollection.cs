// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.ObjectModel;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A collection of <see cref="SolutionPlatform"/>.
    /// </summary>
    public sealed class SolutionPlatformCollection : KeyedCollection<PlatformType, SolutionPlatform>
    {
        protected override PlatformType GetKeyForItem(SolutionPlatform item)
        {
            return item.Type;
        }
    }
}
