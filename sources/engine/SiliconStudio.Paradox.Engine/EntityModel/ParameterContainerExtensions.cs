// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Diffs;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Serializers;
using SiliconStudio.Core.IO;
using System.Diagnostics;

namespace SiliconStudio.Paradox.EntityModel
{
    public class PropertyContainerTypeInfo
    {
        public ParameterCollection DefaultParameters { get; set; }
        public Dictionary<ParameterKey, FieldInfo> Keys { get; set; }
    }

    public class ParameterContainerExtensions
    {
        public static SerializerSelector DefaultSceneSerializerSelector;

        static ParameterContainerExtensions()
        {
            DefaultSceneSerializerSelector = new SerializerSelector()
                .RegisterProfile("Default")
                .RegisterProfile("Asset");
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