// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// Extensions for the <see cref="AssetDependencyManager"/>
    /// </summary>
    public static class AssetDependencyManagerExtensions
    {
        public static AssetDependencies Find(this AssetDependencyManager manager, AssetItem item)
        {
            if (item == null) throw new ArgumentNullException("item");
            return manager.FindDependencySet(item.Id);
        }

        public static IEnumerable<IReference> FindMissingReferences(this AssetDependencyManager manager, AssetItem item)
        {
            if (item == null) throw new ArgumentNullException("item");
            return manager.FindMissingReferences(item.Id);
        }
    }
}
