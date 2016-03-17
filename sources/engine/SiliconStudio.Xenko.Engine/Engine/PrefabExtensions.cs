// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Engine.Design;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Extensions for <see cref="Prefab"/>
    /// </summary>
    public static class PrefabExtensions
    {
        /// <summary>
        /// Instantiates entities from a prefab that can be later added to a <see cref="Scene"/>.
        /// </summary>
        /// <param name="prefab">The prefab to intantiate the entities from</param>
        /// <returns>A collection of entities extracted from the specified prefab</returns>
        public static FastCollection<Entity> Instantiate(this Prefab prefab)
        {
            var newPrefab = EntityCloner.Clone(prefab);
            return newPrefab.Entities;
        }
    }
}