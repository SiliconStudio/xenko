// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Entities
{
    /// <summary>
    /// Base class for entity assets (<see cref="SceneAsset"/> and <see cref="EntityAsset"/>)
    /// </summary>
    [DataContract()]
    public abstract class EntityAssetBase : Asset, IAssetPartContainer
    {
        protected EntityAssetBase()
        {
            Hierarchy = new EntityHierarchyData();
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [DataMember(20)]
        public EntityHierarchyData Hierarchy { get; set; }

        /// <summary>
        /// The various <see cref="EntityAsset"/> that are instantiated in this one.
        /// </summary>
        [DataMemberIgnore]
        [Obsolete]
        public Dictionary<Guid, EntityBase> AssetBases = new Dictionary<Guid, EntityBase>();

        public override Asset CreateChildAsset(string location)
        {
            var newAsset = (EntityAssetBase)base.CreateChildAsset(location);

            // CAUTION: We need to re-add entities to the list as we are going to change their ids
            // (and the Hierarchy.Entities list is ordered by Id, so they should not be changed after the entity has been added)
            var newEntities = new List<EntityDesign>(newAsset.Hierarchy.Entities);
            newAsset.Hierarchy.Entities.Clear();

            // Process entities to create new ids for entities and base id
            for (int i = 0; i < Hierarchy.Entities.Count; i++)
            {
                var oldEntityDesign = Hierarchy.Entities[i];
                var newEntityDesign = newEntities[i];
                // Assign a new guid
                newEntityDesign.Entity.Id = Guid.NewGuid();

                // Store the baseid of the new version
                newEntityDesign.Design.BaseId = oldEntityDesign.Entity.Id;

                // If entity is root, update RootEntities
                // TODO: might not be optimal if many root entities (should use dictionary and second pass on RootEntities)
                int indexRoot = newAsset.Hierarchy.RootEntities.IndexOf(oldEntityDesign.Entity.Id);
                if (indexRoot >= 0)
                {
                    newAsset.Hierarchy.RootEntities[indexRoot] = newEntityDesign.Entity.Id;
                }

                newAsset.Hierarchy.Entities.Add(newEntityDesign);
            }

            return newAsset;
        }

        public override MergeResult Merge(Asset baseAsset, Asset newBase, List<AssetBasePart> newBaseParts)
        {
            var entityMerge = new EntityAssetMerge((EntityAssetBase)baseAsset, this, (EntityAssetBase)newBase, newBaseParts);
            return entityMerge.Merge();
        }

        public IEnumerable<AssetPart> CollectParts()
        {
            foreach (var entityDesign in Hierarchy.Entities)
            {
                yield return new AssetPart(entityDesign.Entity.Id, entityDesign.Design.BaseId);
            }
        }

        public bool ContainsPart(Guid id)
        {
            return Hierarchy.Entities.ContainsKey(id);
        }
    }

    [DataContract("EntityBase")]
    public class EntityBase
    {
        /// <summary>
        /// The <see cref="EntityAsset"/> base.
        /// </summary>
        public AssetBase Base;

        public Guid SourceRoot;

        /// <summary>
        /// Maps <see cref="Entity.Id"/> from this asset to base asset one.
        /// </summary>
        public Dictionary<Guid, Guid> IdMapping;
    }
}