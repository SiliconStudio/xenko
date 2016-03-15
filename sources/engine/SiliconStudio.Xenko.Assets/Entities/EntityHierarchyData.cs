// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    /// <summary>
    /// Contains design data used by an <see cref="Entity"/>
    /// </summary>
    [DataContract("EntityDesignData")]
    [NonIdentifiable]
    public class EntityDesignData
    {
        public EntityDesignData()
        {
        }

        public EntityDesignData(string folder)
        {
            Folder = folder;
        }

        /// <summary>
        /// Gets or sets the folder where the entity is attached (folder is relative to parent folder). If null, the entity doesn't belong to a folder.
        /// </summary>
        [DataMember(10)]
        [DefaultValue(null)]
        public string Folder { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the base entity in case of prefabs. If null, the entity is not a prefab.
        /// </summary>
        [DataMember(20)]
        [DefaultValue(null)]
        public Guid? BaseId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the part group in case of prefabs. If null, the entity doesn't belong to a part.
        /// </summary>
        [DataMember(30)]
        [DefaultValue(null)]
        public Guid? BasePartInstanceId { get; set; }
    }

    /// <summary>
    /// Associate an <see cref="Entity"/> with <see cref="EntityDesignData"/>.
    /// </summary>
    [DataContract("EntityDesign")]
    [NonIdentifiable]
    public class EntityDesign
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EntityDesign"/>.
        /// </summary>
        public EntityDesign()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="EntityDesign"/>.
        /// </summary>
        public EntityDesign(Entity entity)
            : this(entity, new EntityDesignData())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="EntityDesign"/>.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="designData">The design data.</param>
        public EntityDesign(Entity entity, EntityDesignData designData)
        {
            Entity = entity;
            Design = designData;
        }

        /// <summary>
        /// Gets or sets the entity
        /// </summary>
        [DataMember(10)]
        public Entity Entity { get; set; }

        /// <summary>
        /// Gets or sets the design data.
        /// </summary>
        [DataMember(20)]
        public EntityDesignData Design { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"EntityDesign {Entity.Name}";
        }
    }

    [DataContract("EntityHierarchyData")]
    //[ContentSerializer(typeof(DataContentWithEntityReferenceSerializer))]
    public class EntityHierarchyData
    {
        [DataMember(10)]
        public List<Guid> RootEntities { get; private set; }

        [DataMember(20)]
        public EntityCollection Entities { get; private set; }

        public EntityHierarchyData()
        {
            RootEntities = new List<Guid>();
            Entities = new EntityCollection(this);
        }

        [DataSerializer(typeof(KeyedSortedListSerializer<EntityCollection, Guid, EntityDesign>))]
        public sealed class EntityCollection : KeyedSortedList<Guid, EntityDesign>
        {
            private readonly EntityHierarchyData container;

            public EntityCollection(EntityHierarchyData container)
            {
                this.container = container;
            }

            public void Add(Entity entity)
            {
                Add(new EntityDesign(entity, new EntityDesignData()));
            }

            protected override Guid GetKeyForItem(EntityDesign item)
            {
                return item.Entity.Id;
            }
        
            public void AddRange(IEnumerable<EntityDesign> entityDesigns)
            {
                foreach (var entityDesign in entityDesigns)
                {
                    Add(entityDesign);
                }
            }

            public void AddRange(IEnumerable<Entity> entities)
            {
                foreach (var entity in entities)
                {
                    Add(entity);
                }
            }
        }
    }

    //public class DataContentWithEntityReferenceSerializer : DataContentSerializer<EntityHierarchyData>
    //{
    //    public override void Serialize(ContentSerializerContext context, SerializationStream stream, EntityHierarchyData entityHierarchyData)
    //    {
    //        var entityAnalysisResult = new EntityReference.EntityAnalysisResult();
    //        stream.Context.Set(EntityReference.EntityAnalysisResultKey, entityAnalysisResult);
    //
    //        base.Serialize(context, stream, entityHierarchyData);
    //
    //        foreach (var entityReference in entityAnalysisResult.EntityReferences)
    //        {
    //            entityReference.Value = entityHierarchyData.Entities.First(x => x.Id == entityReference.Id);
    //        }
    //
    //        foreach (var entityComponentReference in entityAnalysisResult.EntityComponentReferences)
    //        {
    //            entityComponentReference.Value = entityComponentReference.Entity.Value.Components[entityComponentReference.Component];
    //        }
    //    }
    //}
}