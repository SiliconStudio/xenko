// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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