// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Extensions;
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
        [DataMemberIgnore]
        public EntityHierarchyData Container;

        [DataMember(5)]
        public Guid Id;

        [DataMember(10)]
        public string Name;

        [DataMember(20)]
        public TrackingDictionary<PropertyKey, EntityComponentData> Components = new TrackingDictionary<PropertyKey, EntityComponentData>();

        public EntityData()
        {
            Id = Guid.NewGuid();
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

    public partial class EntityDataConverter : DataConverter<EntityHierarchyData, Entity>
    {
        public override bool CanConstruct
        {
            get { return true; }
        }

        public void SaveEntityData(ConverterContext converterContext, EntityHierarchyData entityHierarchyData, Entity entity)
        {
            var entityData = new EntityData { Name = entity.Name };

            foreach (var component in entity.Tags.Where(x => x.Value is EntityComponent))
            {
                entityData.Components.Add(component.Key, converterContext.ConvertToData<EntityComponentData>(component.Value));
            }

            entityHierarchyData.Entities.Add(entityData);

            foreach (var child in entity.Transformation.Children)
            {
                SaveEntityData(converterContext, entityHierarchyData, child.Entity);
            }
        }

        public override void ConvertToData(ConverterContext converterContext, ref EntityHierarchyData entityHierarchyData, Entity entity)
        {
            entityHierarchyData = new EntityHierarchyData();

            SaveEntityData(converterContext, entityHierarchyData, entity);
        }

        public override void ConvertFromData(ConverterContext converterContext, EntityHierarchyData entityHierarchyData, ref Entity rootEntity)
        {
            // Work in two steps: first construct entities and components, and then convert actual data (to avoid problems with circular references)
            // Note: We could do it in one step (by using ConvertFromDataFlags.Default in first step), but it should help avoid uncontrollable recursion.

            // Keep list of entities. Probably not necessary since converterContext cache should prevent them from being GC anyway.
            var entities = new Entity[entityHierarchyData.Entities.Count];

            // Build entities first
            for (int index = 0; index < entityHierarchyData.Entities.Count; index++)
            {
                var entityData = entityHierarchyData.Entities[index];
                var currentEntity = new Entity(entityData.Name);
                foreach (var component in entityData.Components)
                {
                    currentEntity.Tags.SetObject(component.Key, converterContext.ConvertFromData<EntityComponent>(component.Value, ConvertFromDataFlags.Construct));
                }
                entities[index] = currentEntity;
            }

            // Convert entities
            for (int index = 0; index < entityHierarchyData.Entities.Count; index++)
            {
                var entityData = entityHierarchyData.Entities[index];
                var entity = entities[index];
                foreach (var component in entityData.Components)
                {
                    var entityComponent = (EntityComponent)entity.Tags.Get(component.Key);
                    converterContext.ConvertFromData(component.Value, ref entityComponent, ConvertFromDataFlags.Convert);
                    entity.Tags.SetObject(component.Key, entityComponent);
                }
            }

            rootEntity = entities[0];
        }
    }
}