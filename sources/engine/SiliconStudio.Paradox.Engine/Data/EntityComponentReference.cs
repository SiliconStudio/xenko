// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.EntityModel.Data;

namespace SiliconStudio.Paradox.Data
{
    public static class EntityComponentReference
    {
        public static EntityComponentReference<T> New<T>(Guid id, string location, PropertyKey<T> component) where T : EntityComponent
        {
            return new EntityComponentReference<T>(new ContentReference<EntityData>(id, location), component);
        }
    }

    [DataContract]
    [DataStyle(DataStyle.Compact)]
    public struct EntityComponentReference<T> : IEntityComponentReference where T : EntityComponent
    {
        // TODO: implement a serializer and pass these fields readonly (and their related properties)
        private ContentReference<EntityData> entity;
        private PropertyKey<T> component;

        public EntityComponentReference(ContentReference<EntityData> entity, PropertyKey<T> component)
        {
            this.entity = entity;
            this.component = component;
        }

        PropertyKey IEntityComponentReference.Component { get { return Component; } }

        [DataMemberIgnore]
        public Type ComponentType { get { return typeof(T); } }

        [DataMember(10)]
        public ContentReference<EntityData> Entity { get { return entity; } set { entity = value; } }

        [DataMember(20)]
        public PropertyKey<T> Component { get { return component; } set { component = value; } }
    }
}
