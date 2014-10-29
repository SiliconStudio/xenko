// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;
using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.EntityModel.Data;
using EntityComponentData = SiliconStudio.Paradox.EntityModel.Data.EntityComponentData;

namespace SiliconStudio.Paradox.Data
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

    [DataContract]
    [DataStyle(DataStyle.Compact)]
    [DataSerializer(typeof(EntityReference.Serializer))]
    public sealed class EntityReference
    {
        private Guid id;
        private string name;

        public Guid Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        [DataMemberIgnore]
        public EntityData Value { get; set; }

        public static explicit operator EntityData(EntityReference contentReference)
        {
            if (contentReference == null)
                return null;
            return contentReference.Value;
        }

        public static implicit operator EntityReference(EntityData value)
        {
            return new EntityReference() { Value = value };
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Id, Name);
        }

        public static readonly PropertyKey<EntityAnalysisResult> EntityAnalysisResultKey = new PropertyKey<EntityAnalysisResult>("EntityAnalysisResult", typeof(EntityReference));

        internal class Serializer : DataSerializer<EntityReference>
        {
            public override void Serialize(ref EntityReference obj, ArchiveMode mode, SerializationStream stream)
            {
                if (obj == null)
                    obj = new EntityReference();

                if (mode == ArchiveMode.Deserialize)
                {
                    var entityReferenceContext = stream.Context.Get(EntityAnalysisResultKey);
                    if (entityReferenceContext != null)
                    {
                        entityReferenceContext.EntityReferences.Add(obj);
                    }
                }

                stream.Serialize(ref obj.name);
                stream.Serialize(ref obj.id, mode);
            }
        }

        public class EntityAnalysisResult
        {
            public List<IEntityComponentReference> EntityComponentReferences = new List<IEntityComponentReference>();
            public List<EntityReference> EntityReferences = new List<EntityReference>();
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
