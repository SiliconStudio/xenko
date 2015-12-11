// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Adds an entity as a part asset.
        /// </summary>
        /// <param name="assetPartBase">The entity asset to be used as a part (must be created directly from <see cref="CreateChildAsset"/>)</param>
        /// <param name="rootEntityId">An optional entity id to attach the part to it. If null, the part will be attached to the root entities of this instance</param>
        public void AddPart(EntityAssetBase assetPartBase, Guid? rootEntityId = null)
        {
            if (assetPartBase == null) throw new ArgumentNullException(nameof(assetPartBase));

            // The assetPartBase must be a plain child asset
            if (assetPartBase.Base == null) throw new InvalidOperationException($"Expecting a Base for {nameof(assetPartBase)}");
            if (assetPartBase.BaseParts != null) throw new InvalidOperationException($"Expecting a null BaseParts for {nameof(assetPartBase)}");

            // Check that the assetPartBase contains only entities from its base (no new entity, must be a plain ChildAsset)
            if (assetPartBase.Hierarchy.Entities.Any(entity => !entity.Design.BaseId.HasValue))
            {
                throw new InvalidOperationException("An asset part base must contain only base assets");
            }

            // The instance id will be the id of the assetPartBase
            var instanceId = assetPartBase.Id;
            if (this.BaseParts == null)
            {
                this.BaseParts = new List<AssetBasePart>();
            }

            var basePart = this.BaseParts.FirstOrDefault(basePartIt => basePartIt.Base.Id == this.Id);
            if (basePart == null)
            {
                basePart = new AssetBasePart(assetPartBase.Base);
                this.BaseParts.Add(basePart);
            }
            basePart.InstanceIds.Add(instanceId);

            // If a RootEntityId is given and found in this instance, add them as children of entity
            if (rootEntityId.HasValue && this.Hierarchy.Entities.ContainsKey(rootEntityId.Value))
            {
                var rootEntity = Hierarchy.Entities[rootEntityId.Value];
                foreach (var entityId in assetPartBase.Hierarchy.RootEntities)
                {
                    var entity = assetPartBase.Hierarchy.Entities[entityId];
                    rootEntity.Entity.Transform.Children.Add(entity.Entity.Transform);
                }
            }
            else
            {
                // Else add them as root
                this.Hierarchy.RootEntities.AddRange(assetPartBase.Hierarchy.RootEntities);
            }

            // Add all entities with the correct instance id
            foreach (var entityEntry in assetPartBase.Hierarchy.Entities)
            {
                entityEntry.Design.BasePartInstanceId = instanceId;
                this.Hierarchy.Entities.Add(entityEntry);
            }
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