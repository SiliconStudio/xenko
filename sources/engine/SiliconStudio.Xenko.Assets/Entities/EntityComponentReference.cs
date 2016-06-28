// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    [DataContract]
    [DataStyle(DataStyle.Compact)]
    [NonIdentifiable]
    public sealed class EntityComponentReference : IAssetPartReference
    {
        // TODO: we keep this type here internally to not break existing reference - but it's not used anywhere else. Remove it when writing a dedicated serialized for entity component reference
        [DataContract]
        [DataStyle(DataStyle.Compact)]
        public sealed class EntityReference : IIdentifiable
        {
            public Guid Id { get; set; }
        }
        
        // TODO: implement a serializer and pass these fields readonly (and their related properties)

        [DataMember(10)]
        public EntityReference Entity { get; set; }

        [DataMember(20)]
        public Guid Id { get; set; }

        [DataMemberIgnore]
        public Type InstanceType { get; set; }

        public void FillFromPart(object assetPart)
        {
            var component = (EntityComponent)assetPart;
            Entity = new EntityReference { Id = component.Entity.Id };
            Id = IdentifiableHelper.GetId(component);
        }

        public object GenerateProxyPart(Type partType)
        {
            var component = (EntityComponent)Activator.CreateInstance(partType);
            component.Entity = new Entity { Id = Entity.Id };
            IdentifiableHelper.SetId(component, Id);
            return component;
        }
    }
}
