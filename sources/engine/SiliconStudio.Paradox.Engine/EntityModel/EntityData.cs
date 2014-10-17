// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Converters;

namespace SiliconStudio.Paradox.EntityModel.Data
{
    public partial class EntityComponentData
    {
        /// <summary>
        /// Entity will get updated when added to EntityData.Components.
        /// </summary>
        [DataMemberIgnore]
        public EntityData Entity;
    }

    // Entities as they are stored in the git shared scene file
    public partial class EntityData
    {
        [DataMember(10)]
        public string Name;

        [DataMember(20)]
        public TrackingDictionary<PropertyKey, EntityComponentData> Components = new TrackingDictionary<PropertyKey, EntityComponentData>();

        public EntityData()
        {
            Components.CollectionChanged += Components_CollectionChanged;
        }

        void Components_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            // Remove entity owner from previous EntityComponentData.
            if (e.OldItem is EntityComponentData)
            {
                ((EntityComponentData)e.OldItem).Entity = null;
            }

            // Set entity owner to this new EntityComponentData.
            if (e.Item is EntityComponentData)
            {
                ((EntityComponentData)e.Item).Entity = this;
            }
        }
    }

    public partial class EntityDataConverter : DataConverter<EntityData, Entity>
    {
        public override bool CanConstruct
        {
            get { return true; }
        }

        public override void ConstructFromData(ConverterContext converterContext, EntityData entityData, ref Entity entity)
        {
            entity = new Entity(entityData.Name);
            foreach (var component in entityData.Components)
            {
                entity.Tags.SetObject(component.Key, converterContext.ConvertFromData<EntityComponent>(component.Value, ConvertFromDataFlags.Construct));
            }
        }

        public override void ConvertToData(ConverterContext converterContext, ref EntityData entityData, Entity entity)
        {
            entityData = new EntityData
                {
                    Name = entity.Name,
                };

            foreach (var component in entity.Tags.Where(x => x.Value is EntityComponent))
            {
                entityData.Components.Add(component.Key, converterContext.ConvertToData<EntityComponentData>(component.Value));
            }
        }

        public override void ConvertFromData(ConverterContext converterContext, EntityData entityData, ref Entity entity)
        {
            foreach (var component in entityData.Components)
            {
                var entityComponent = (EntityComponent)entity.Tags.Get(component.Key);
                converterContext.ConvertFromData(component.Value, ref entityComponent, ConvertFromDataFlags.Convert);
                entity.Tags.SetObject(component.Key, entityComponent);
            }
        }
    }
}