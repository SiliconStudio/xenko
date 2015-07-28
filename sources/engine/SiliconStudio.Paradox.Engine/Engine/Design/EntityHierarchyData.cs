// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Paradox.Engine.Design
{
    [DataContract]
    //[ContentSerializer(typeof(DataContentWithEntityReferenceSerializer))]
    public class EntityHierarchyData
    {
        [DataMember(10)]
        public List<Guid> RootEntities { get; private set; }

        [DataMember(20)]
        public EntityCollection Entities { get; private set; }

        [DataMember(30)]
        public SceneSettings SceneSettings { get; private set; }

        public EntityHierarchyData()
        {
            RootEntities = new List<Guid>();
            Entities = new EntityCollection(this);
            SceneSettings = new SceneSettings();
        }

        [DataSerializer(typeof(KeyedSortedListSerializer<EntityCollection, Guid, Entity>))]
        public sealed class EntityCollection : KeyedSortedList<Guid, Entity>
        {
            private readonly EntityHierarchyData container;

            public EntityCollection(EntityHierarchyData container)
            {
                this.container = container;
            }
        
            protected override Guid GetKeyForItem(Entity item)
            {
                return item.Id;
            }
        
            protected override void InsertItem(int index, Entity item)
            {
                //item.Container = container;
                base.InsertItem(index, item);
            }
        
            protected override void RemoveItem(int index)
            {
                var item = items[index];
                base.RemoveItem(index);
                //item.Container = null;
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