// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Engine.Design
{
    public class ParameterContainerExtensions
    {
        public static SerializerSelector DefaultSceneSerializerSelector;

        static ParameterContainerExtensions()
        {
            DefaultSceneSerializerSelector = new SerializerSelector("Default", "Content");
        }

        public static HashSet<Entity> CollectEntityTree(Entity entity)
        {
            var entities = new HashSet<Entity>();
            CollectEntityTreeHelper(entity, entities);
            return entities;
        }

        private static void CollectEntityTreeHelper(Entity entity, HashSet<Entity> entities)
        {
            // Already processed
            if (!entities.Add(entity))
                return;

            var transformationComponent = entity.Transform;
            if (transformationComponent != null)
            {
                foreach (var child in transformationComponent.Children)
                {
                    CollectEntityTreeHelper(child.Entity, entities);
                }
            }
        }
    }
}