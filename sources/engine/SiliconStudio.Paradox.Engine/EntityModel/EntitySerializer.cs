// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.EntityModel
{
    public class EntitySerializer : DataSerializer<Entity>, IDataSerializerInitializer
    {
        private DataSerializer<EntityData> entityDataSerializer;

        /// <inheritdoc/>
        public void Initialize(SerializerSelector serializerSelector)
        {
            entityDataSerializer = MemberSerializer<EntityData>.Create(serializerSelector);
        }

        /// <inheritdoc/>
        public override void PreSerialize(ref Entity entity, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Deserialize && entity == null)
            {
                // Generate a new entity without creating a random Id; it will be set during deserialization
                entity = new Entity(null, false);
            }
        }

        /// <inheritdoc/>
        public override void Serialize(ref Entity entity, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                var entityData = new EntityData
                    {
                        Name = entity.Name,
                        Guid = entity.Id,
                        Components = entity.Components
                            .Where(x => x.Value is EntityComponent)
                            .ToDictionary(x => x.Key, x => (EntityComponent)x.Value),
                    };
                entityDataSerializer.Serialize(ref entityData, mode, stream);
            }
            else if (mode == ArchiveMode.Deserialize)
            {
                EntityData entityData = null;
                entityDataSerializer.Serialize(ref entityData, mode, stream);
                entity.Name = entityData.Name;
                entity.Id = entityData.Guid;

                foreach (var component in entityData.Components)
                {
                    entity.Components.SetObject(component.Key, component.Value);
                }
            }
        }

        [DataContract]
        public class EntityData
        {
            public Guid Guid;
            public string Name;
            public Dictionary<PropertyKey, EntityComponent> Components;
        }
    }
}