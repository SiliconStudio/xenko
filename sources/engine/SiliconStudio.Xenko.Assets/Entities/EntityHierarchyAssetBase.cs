// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    /// <summary>
    /// Base class for entity assets (<see cref="SceneAsset"/> and <see cref="PrefabAsset"/>)
    /// </summary>
    [DataContract]
    [AssetPartReference(typeof(Entity), typeof(EntityComponent))]
    [AssetPartReference(typeof(EntityComponent), ReferenceType = typeof(EntityComponentReference))]
    public abstract partial class EntityHierarchyAssetBase : AssetCompositeHierarchy<EntityDesign, Entity>
    {
        /// <summary>
        /// Dumps this asset to a writer for debug purposes.
        /// </summary>
        /// <param name="writer">A text writer output</param>
        /// <param name="name">Name of this asset</param>
        /// <returns><c>true</c> if the dump was sucessful, <c>false</c> otherwise</returns>
        public bool DumpTo(TextWriter writer, string name)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            writer.WriteLine();
            writer.WriteLine("*************************************");
            writer.WriteLine($"{GetType().Name}: {name}");
            writer.WriteLine("=====================================");
            return Hierarchy?.DumpTo(writer) ?? false;
        }

        /// <inheritdoc/>
        public override MergeResult Merge(Asset baseAsset, Asset newBase, List<AssetBase> newBaseParts, UFile debugLocation = null)
        {
            var entityMerge = new PrefabAssetMerge((EntityHierarchyAssetBase)baseAsset, this, (EntityHierarchyAssetBase)newBase, newBaseParts, debugLocation);
            return entityMerge.Merge();
        }

        /// <inheritdoc/>
        public override Entity GetParent(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            return entity.Transform.Parent?.Entity;
        }

        /// <inheritdoc/>
        public override int IndexOf(Entity part)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            var parent = GetParent(part);
            return parent?.Transform.Children.IndexOf(part.Transform) ?? Hierarchy.RootPartIds.IndexOf(part.Id);
        }

        /// <inheritdoc/>
        public override Entity GetChild(Entity part, int index)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            return part.Transform.Children[index].Entity;
        }

        /// <inheritdoc/>
        public override int GetChildCount(Entity part)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            return part.Transform.Children.Count;
        }

        /// <inheritdoc/>
        public override IEnumerable<Entity> EnumerateChildParts(Entity entity, bool isRecursive)
        {
            if (entity.Transform == null)
                return Enumerable.Empty<Entity>();

            var enumerator = isRecursive ? entity.Transform.Children.DepthFirst(t => t.Children) : entity.Transform.Children;
            return enumerator.Select(t => t.Entity);
        }

        /// <summary>
        /// Gets a mapping between a base and the list of instance actually used
        /// </summary>
        /// <param name="baseParts">The list of baseParts to use. If null, use the parts from this instance directly.</param>
        /// <returns>A mapping between a base asset and the list of instance actually used for inherited parts by composition</returns>
        public Dictionary<EntityHierarchyAssetBase, List<Guid>> GetBasePartInstanceIds(List<AssetBase> baseParts = null)
        {
            if (baseParts == null)
            {
                baseParts = BaseParts;
            }

            var mapBaseToInstanceIds = new Dictionary<EntityHierarchyAssetBase, List<Guid>>();
            if (baseParts == null)
            {
                return mapBaseToInstanceIds;
            }

            // This method is recovering links between a derived entity from a base part.
            // This is done in 2 steps:


            // Step 1) build the map <mapBasePartInstanceIdToBasePart>: <basePartInstanceId> => <base part asset>  
            //
            // - for each entity in the hierarchy of this instance
            //   - Check if the entity has a <baseId> and a <basePartInstanceId>
            //   - If yes, the entity is coming from a base part
            //       - Find which AssetBase (actually EntityHierarchyAssetBase), is containing the <basePartInstanceId>
            //       - We can then associate 
            var mapBasePartInstanceIdToBasePart = new Dictionary<Guid, EntityHierarchyAssetBase>();
            foreach (var entityIt in Hierarchy.Parts)
            {
                if (entityIt.BaseId.HasValue && entityIt.BasePartInstanceId.HasValue)
                {
                    var basePartInstanceId = entityIt.BasePartInstanceId.Value;
                    EntityHierarchyAssetBase existingAssetBase;
                    if (!mapBasePartInstanceIdToBasePart.TryGetValue(basePartInstanceId, out existingAssetBase))
                    {
                        var baseId = entityIt.BaseId.Value;
                        foreach (var basePart in baseParts)
                        {
                            var assetBase = (EntityHierarchyAssetBase)basePart.Asset;
                            if (assetBase.ContainsPart(baseId))
                            {
                                existingAssetBase = assetBase;
                                break;
                            }
                        }

                        if (existingAssetBase != null)
                        {
                            mapBasePartInstanceIdToBasePart.Add(basePartInstanceId, existingAssetBase);
                        }
                    }
                }
            }

            // Step 2) build the resulting reverse map <mapBaseToInstanceIds>: <base part asset> => list of <basePartInstanceId>
            //
            // - We simply build this map by using the mapBasePartInstanceIdToBasePart
            foreach (var it in mapBasePartInstanceIdToBasePart)
            {
                List<Guid> ids;
                if (!mapBaseToInstanceIds.TryGetValue(it.Value, out ids))
                {
                    ids = new List<Guid>();
                    mapBaseToInstanceIds.Add(it.Value, ids);
                }
                ids.Add(it.Key);
            }
            return mapBaseToInstanceIds;
        }

        /// <inheritdoc/>
        protected override void ClearPartReferences(AssetCompositeHierarchyData<EntityDesign, Entity> clonedHierarchy)
        {
            // set to null reference outside of the sub-tree
            var tempAsset = new PrefabAsset { Hierarchy = clonedHierarchy };
            tempAsset.FixupReferences();
        }

        /// <inheritdoc/>
        protected override void PostClonePart(Entity part)
        {
            // disconnect the cloned entity
            part.Transform.Parent = null;
        }

        /// <inheritdoc/>
        protected override object ResolvePartReference(object partReference)
        {
            var entityComponentReference = partReference as EntityComponent;
            if (entityComponentReference != null)
            {
                var containingEntity = entityComponentReference.Entity;
                if (containingEntity == null)
                {
                    throw new InvalidOperationException("Found a reference to a component which doesn't have any entity");
                }

                var realEntity = (Entity)base.ResolvePartReference(containingEntity);
                if (realEntity == null)
                    return null;

                var componentId = IdentifiableHelper.GetId(entityComponentReference);
                var realComponent = realEntity.Components.FirstOrDefault(c => IdentifiableHelper.GetId(c) == componentId);
                return realComponent;
            }

            return base.ResolvePartReference(partReference);
        }
    }
}
