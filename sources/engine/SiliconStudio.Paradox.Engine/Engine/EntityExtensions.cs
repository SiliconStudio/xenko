// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Engine.Design;

namespace SiliconStudio.Paradox.Engine
{
    public static class EntityExtensions
    {
        /// <summary>
        /// Deep clone of this entity.
        /// </summary>
        /// <param name="entity">The entity to clone</param>
        /// <returns>The cloned entity</returns>
        public static Entity Clone(this Entity entity)
        {
            return EntityCloner.Clone(entity);
        }
    }
}