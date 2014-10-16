// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SiliconStudio.Paradox.EntityModel.Data;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

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

        public override void Serialize(ref Entity entity, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                var entityData = new EntityData
                    {
                        Name = entity.Name,
                        Components = entity.Tags
                            .Where(x => x.Value is EntityComponent)
                            .ToDictionary(x => x.Key, x => (EntityComponent)x.Value),
                    };
                entityDataSerializer.Serialize(ref entityData, mode, stream);
            }
            else if (mode == ArchiveMode.Deserialize)
            {
                if (entity == null)
                    entity = new Entity();

                EntityData entityData = null;
                entityDataSerializer.Serialize(ref entityData, mode, stream);
                entity.Name = entityData.Name;

                foreach (var component in entityData.Components)
                {
                    entity.Tags.SetObject(component.Key, component.Value);
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