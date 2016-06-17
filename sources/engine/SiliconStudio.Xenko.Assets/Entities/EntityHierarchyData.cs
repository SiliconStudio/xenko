// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    /// <summary>
    /// Associate an <see cref="Entity"/> with design-time data.
    /// </summary>
    [DataContract("EntityDesign")]
    [NonIdentifiable]
    public class EntityDesign : IAssetPartDesign<Entity>
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
        /// <param name="entity">The entity</param>
        public EntityDesign(Entity entity)
        {
            Entity = entity;
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

        /// <summary>
        /// Gets or sets the entity
        /// </summary>
        [DataMember(40)]
        public Entity Entity { get; set; }

        /// <inheritdoc/>
        Entity IAssetPartDesign<Entity>.Part => Entity;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"EntityDesign [{Entity.Name}]";
        }
    }



    [DataContract("EntityHierarchyData")]
    //[ContentSerializer(typeof(DataContentWithEntityReferenceSerializer))]
    public class EntityHierarchyData
    {
        [DataMember(10)]
        public List<Guid> RootEntities { get; } = new List<Guid>();

        [DataMember(20)]
        public AssetPartCollection<EntityDesign> Entities { get; } = new AssetPartCollection<EntityDesign>();
    }

    public static class EntityHierarchyDataExtensions
    {
        /// <summary>
        /// Helper method to dump this hierarchy to a text output
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="writer"></param>
        /// <returns><c>true</c> if the dump was sucessful, <c>false</c> otherwise</returns>
        public static bool DumpTo(this EntityHierarchyData hierarchy, TextWriter writer)
        {
            bool result = true;
            writer.WriteLine("***************");
            writer.WriteLine($"RootEntities [{hierarchy.RootEntities.Count}]");
            writer.WriteLine("===============");
            for (int i = 0; i < hierarchy.RootEntities.Count; i++)
            {
                var id = hierarchy.RootEntities[i];
                if (!hierarchy.Entities.ContainsKey(id))
                {
                    result = false;
                }
                writer.WriteLine(hierarchy.Entities.ContainsKey(id) ? $"{id} => {hierarchy.Entities[id].Entity}" : $"{id} => ERROR - Entity not found in [Entities]");
            }

            writer.WriteLine("***************");
            writer.WriteLine($"Entities [{hierarchy.Entities.Count}]");
            writer.WriteLine("===============");
            for (int i = 0; i < hierarchy.Entities.Count; i++)
            {
                var entityEntry = hierarchy.Entities[i];

                writer.Write($"{entityEntry.Entity.Id} => {entityEntry.Entity}");

                if (entityEntry.BaseId != null)
                {
                    writer.Write($" Base: {entityEntry.BaseId}");
                }

                if (entityEntry.BasePartInstanceId != null)
                {
                    writer.Write($" BasePartInstanceId: {entityEntry.BasePartInstanceId}");
                }
                writer.WriteLine();

                foreach (var child in entityEntry.Entity.Transform.Children)
                {

                    writer.Write($"  - {child.Entity.Id} => {child.Entity.Name}");
                    if (!hierarchy.Entities.ContainsKey(child.Entity.Id))
                    {
                        writer.Write(" <= ERROR, Entity not found in [Entities]");
                        result = false;

                    }
                    writer.WriteLine();
                }
            }
            return result;
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