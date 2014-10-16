// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Extensions
{
    public static class EntityExtensions
    {
        public static IEnumerable<T> Components<T>(this IEnumerable<Entity> entities, PropertyKey<T> key) where T : EntityComponent
        {
            return entities.Select(x => x.Get(key)).Where(x => x != null);
        }

        public static IEnumerable<Entity> GetChildren(this Entity entity)
        {
            var transformationComponent = entity.Transformation;
            if (transformationComponent != null)
            {
                foreach (var child in transformationComponent.Children)
                {
                    yield return child.Entity;
                }
            }
        }
    }
}