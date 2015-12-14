// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    [DataContract]
    [DataStyle(DataStyle.Compact)]
    [NonIdentifitable]
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
            this.component = entityComponent.GetDefaultKey();
            this.value = entityComponent;
        }

        PropertyKey IEntityComponentReference.Component { get { return Component; } }

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

        public static EntityComponentReference New(EntityComponent entityComponent)
        {
            return new EntityComponentReference(entityComponent);
        }
    }
}
