// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SharpYaml.Serialization;
using SharpYaml.Serialization.Serializers;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Debugger.Target
{
    /// <summary>
    /// When serializing/deserializing Yaml for live objects, this serializer will handle those objects as reference (similar to Clone serializer).
    /// </summary>
    [YamlSerializerFactory]
    public class CloneReferenceSerializer : ObjectSerializer
    {
        // TODO: We might want to share some of the recursive logic with PrefabAssetSerializer?
        // However, ThreadStatic would still need to be separated...
        [ThreadStatic]
        private static int recursionLevel;

        /// <summary>
        /// The list of live references during that serialization/deserialization cycle.
        /// </summary>
        [ThreadStatic] internal static List<object> References;

        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            if (CanVisit(typeDescriptor.Type))
                return this;

            return null;
        }

        private bool CanVisit(Type type)
        {
            // Also handles Entity, EntityComponent and Script
            return ContentReferenceSerializer.IsReferenceType(type)
                   || type == typeof(Entity) || typeof(Entity).IsAssignableFrom(type) || typeof(EntityComponent).IsAssignableFrom(type);
        }

        /// <inheritdoc/>
        protected override void CreateOrTransformObject(ref ObjectContext objectContext)
        {
            if (recursionLevel >= 2)
            {
                // We are inside a Script
                // Transform everything into CloneReference for both serialization and deserialization
                if (objectContext.SerializerContext.IsSerializing)
                {
                    var index = References.Count;
                    objectContext.Tag = objectContext.Settings.TagTypeRegistry.TagFromType(objectContext.Instance.GetType());
                    References.Add(objectContext.Instance);
                    objectContext.Instance = new CloneReference { Id = index };
                }
                else
                {
                    objectContext.Instance = new CloneReference();
                }
            }

            base.CreateOrTransformObject(ref objectContext);
        }

        /// <inheritdoc/>
        protected override void TransformObjectAfterRead(ref ObjectContext objectContext)
        {
            if (recursionLevel >= 2)
            {
                // We are inside a Script
                if (!objectContext.SerializerContext.IsSerializing)
                {
                    if (objectContext.Instance is CloneReference)
                    {
                        objectContext.Instance = References[((CloneReference)objectContext.Instance).Id];
                        return;
                    }
                }
            }

            base.TransformObjectAfterRead(ref objectContext);
        }

        /// <inheritdoc/>
        public override void WriteYaml(ref ObjectContext objectContext)
        {
            recursionLevel++;

            try
            {
                base.WriteYaml(ref objectContext);
            }
            finally
            {
                recursionLevel--;
            }
        }

        /// <inheritdoc/>
        public override object ReadYaml(ref ObjectContext objectContext)
        {
            recursionLevel++;

            try
            {
                return base.ReadYaml(ref objectContext);
            }
            finally
            {
                recursionLevel--;
            }
        }

        /// <summary>
        /// Helper class used by CloneReferenceSerializer
        /// </summary>
        internal class CloneReference
        {
            public int Id;
        }
    }
}