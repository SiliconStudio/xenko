// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;
using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.EntityModel.Data;

namespace SiliconStudio.Paradox.EntityModel
{
    public struct EntityPathReference
    {
        public EntityReference[] Path { get; set; }
    }

    [DataContract]
    [DataStyle(DataStyle.Compact)]
    [DataSerializerGlobal(typeof(EntityComponentReferenceSerializer), typeof(EntityComponentReference))]
    public sealed class EntityComponentReference : IEntityComponentReference
    {
        // TODO: implement a serializer and pass these fields readonly (and their related properties)
        internal EntityReference entity;
        internal PropertyKey component;

        private EntityComponent value;

        public EntityComponentReference()
        {
        }

        public EntityComponentReference(EntityReference entity, PropertyKey component)
        {
            this.entity = entity;
            this.component = component;
            this.value = null;
        }

        public EntityComponentReference(EntityComponent entityComponent)
        {
            this.entity = new EntityReference { Id = entityComponent.Entity.Id };
            this.component = entityComponent.DefaultKey;
            this.value = entityComponent;
        }

        PropertyKey IEntityComponentReference.Component { get { return Component; } set { Component = value; } }

        [DataMemberIgnore]
        public Type ComponentType { get { return component.PropertyType; } }

        [DataMember(10)]
        public EntityReference Entity { get { return entity; } set { entity = value; } }

        [DataMember(20)]
        public PropertyKey Component { get { return component; } set { component = value; } }

        [DataMemberIgnore]
        public EntityComponent Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public static EntityComponentReference New(EntityData entityData, PropertyKey component)
        {
            return new EntityComponentReference(entityData, component);
        }

        public static EntityComponentReference New(EntityComponent entityComponent)
        {
            return new EntityComponentReference(entityComponent);
        }
    }

    internal class EntityComponentReferenceSerializer : DataSerializer<EntityComponentReference>
    {
        public override void Serialize(ref EntityComponentReference obj, ArchiveMode mode, SerializationStream stream)
        {
            if (obj == null)
                obj = new EntityComponentReference();

            if (mode == ArchiveMode.Deserialize)
            {
                var entityReferenceContext = stream.Context.Get(EntityReference.EntityAnalysisResultKey);
                if (entityReferenceContext != null)
                {
                    entityReferenceContext.EntityComponentReferences.Add(obj);
                }
            }

            stream.Serialize(ref obj.entity, mode);
            stream.Serialize(ref obj.component, mode);
        }
    }
}
