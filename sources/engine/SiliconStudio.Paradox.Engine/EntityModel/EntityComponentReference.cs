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
    public static class EntityComponentReference
    {
        public static EntityComponentReference<T> New<T>(EntityData entityData, PropertyKey<T> component) where T : EntityComponent
        {
            return new EntityComponentReference<T>(entityData, component);
        }

        public static EntityComponentReference<T> New<T>(EntityComponentData entityComponent) where T : EntityComponent
        {
            return new EntityComponentReference<T>(entityComponent);
        }
    }

    public struct EntityPathReference
    {
        public EntityReference[] Path { get; set; }
    }

    [DataContract]
    [DataStyle(DataStyle.Compact)]
    [DataSerializerGlobal(typeof(EntityComponentReferenceSerializer<>), typeof(EntityComponentReference<>), DataSerializerGenericMode.GenericArguments)]
    public sealed class EntityComponentReference<T> : IEntityComponentReference where T : EntityComponent
    {
        // TODO: implement a serializer and pass these fields readonly (and their related properties)
        internal EntityReference entity;
        internal PropertyKey<T> component;

        private EntityComponentData value;

        public EntityComponentReference()
        {
        }

        public EntityComponentReference(EntityReference entity, PropertyKey<T> component)
        {
            this.entity = entity;
            this.component = component;
            this.value = null;
        }

        public EntityComponentReference(EntityComponentData entityComponent)
        {
            this.entity = new EntityReference();
            this.component = null;
            this.value = entityComponent;
        }

        PropertyKey IEntityComponentReference.Component { get { return Component; } set { Component = (PropertyKey<T>)value; } }

        [DataMemberIgnore]
        public Type ComponentType { get { return typeof(T); } }

        [DataMember(10)]
        public EntityReference Entity { get { return entity; } set { entity = value; } }

        [DataMember(20)]
        public PropertyKey<T> Component { get { return component; } set { component = value; } }

        [DataMemberIgnore]
        public EntityComponentData Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }

    internal class EntityComponentReferenceSerializer<T> : DataSerializer<EntityComponentReference<T>> where T : EntityComponent
    {
        public override void Serialize(ref EntityComponentReference<T> obj, ArchiveMode mode, SerializationStream stream)
        {
            if (obj == null)
                obj = new EntityComponentReference<T>();

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
