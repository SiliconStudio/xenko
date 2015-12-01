// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Shaders.Utility;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    internal class EntityAssetMerge
    {
        public EntityAssetMerge()
        {
        }

        public MergeResult Merge(EntityAssetBase baseAsset, EntityAssetBase newAsset, EntityAssetBase newBase, List<AssetItem> newBaseParts)
        {
            // Prepare mappings for base
            var baseEntities = new Dictionary<Guid, EntityRemapEntry>();
            MapEntities(baseAsset?.Hierarchy, baseEntities);

            // Prepare mapping for new asset
            var newEntities = new Dictionary<Guid, EntityRemapEntry>();
            MapEntities(newAsset.Hierarchy, newEntities);
            if (newAsset.BaseParts != null)
            {
                foreach (var partItem in newAsset.BaseParts)
                {
                    var assetPart = (EntityAssetBase)partItem.Asset;
                    MapEntities(assetPart.Hierarchy, baseEntities);
                }
            }

            // Prepare mapping for new base
            var newBaseEntities = new Dictionary<Guid, EntityRemapEntry>();
            MapEntities(newBase?.Hierarchy, newBaseEntities);
            if (newBaseParts != null)
            {
                foreach (var partItem in newBaseParts)
                {
                    var assetPart = (EntityAssetBase)partItem.Asset;
                    MapEntities(assetPart.Hierarchy, newBaseEntities);
                }
            }

            // Compute Entities Added by newbase (not present in base)
            var entitiesAddedByNewBase = new HashSet<Guid>();
            foreach (var entityFromNewBase in newBaseEntities)
            {
                var entityId = entityFromNewBase.Value.EntityDesign.Entity.Id;
                if (!baseEntities.ContainsKey(entityId))
                {
                    entitiesAddedByNewBase.Add(entityId);
                }
            }

            // Compute Entities Removed in newbase (present in base)
            var entitiesRemovedInNewBase = new HashSet<Guid>();
            foreach (var entityFromBase in baseEntities)
            {
                var entityId = entityFromBase.Value.EntityDesign.Entity.Id;
                if (newBaseEntities.ContainsKey(entityId))
                {
                    entitiesRemovedInNewBase.Add(entityId);
                }
            }

            // Prebuild the list of entities that we will have to remove
            var entitiesToRemoveFromNew = new HashSet<Entity>(ReferenceEqualityComparer<Entity>.Default);
            foreach (var entityEntry in newEntities.ToList())
            {
                var entityDesign = entityEntry.Value.EntityDesign;
                var newEntity = entityDesign.Entity;

                if (entityDesign.Design.BaseId.HasValue)
                {
                    var baseId = entityDesign.Design.BaseId.Value;
                    if (entitiesRemovedInNewBase.Contains(baseId))
                    {
                        entitiesToRemoveFromNew.Add(newEntity);

                        // Else the entity has been removed
                        newEntities.Remove(entityEntry.Key);
                    }
                }
            }

            var result = new MergeResult(newAsset);

            // Visit all existing entities on asset
            foreach (var entityEntry in newEntities) // use ToList as we can modify the list while iterating
            {
                var entityDesign = entityEntry.Value.EntityDesign;
                var newEntity = entityDesign.Entity;

                if (entityDesign.Design.BaseId.HasValue)
                {
                    var baseId = entityDesign.Design.BaseId.Value;

                    EntityRemapEntry baseRemap;
                    EntityRemapEntry newBaseRemap;
                    baseEntities.TryGetValue(baseId, out baseRemap);
                    newBaseEntities.TryGetValue(baseId, out newBaseRemap);
                    if (baseRemap != null && newBaseRemap != null)
                    {
                        var baseEntity = baseRemap.EntityDesign.Entity;
                        var newBaseEntity = newBaseRemap.EntityDesign.Entity;

                        var diff = new AssetDiff(baseEntity, newEntity, newBaseEntity)
                        {
                            UseOverrideMode = true,
                        };

                        // For entities and components, we will visit only the members of the first level (first entity, or first component) 
                        // but not recursive one (in case a component reference another entity or component)
                        diff.CustomVisitorsBase.Add(new SingleLevelVisitor(typeof(Entity), false));
                        diff.CustomVisitorsBase.Add(new SingleLevelVisitor(typeof(EntityComponent), false));

                        diff.CustomVisitorsAsset1.Add(new SingleLevelVisitor(typeof(Entity), false));
                        diff.CustomVisitorsAsset1.Add(new SingleLevelVisitor(typeof(EntityComponent), false));

                        diff.CustomVisitorsAsset2.Add(new SingleLevelVisitor(typeof(Entity), false));
                        diff.CustomVisitorsAsset2.Add(new SingleLevelVisitor(typeof(EntityComponent), false));

                        // Merge assets
                        var localResult = AssetMerge.Merge(diff, AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1);
                        localResult.CopyTo(result);
                    }
                    else
                    {
                        // log a warning?
                    }
                }

                // If we have any entities to remove, we need to go through all references and remove them
                if (entitiesToRemoveFromNew.Count > 0)
                {
                    // Remove references to components that were removed
                    var entityVisitor = new SingleLevelVisitor(typeof(Entity), true);
                    var entityComponentVisitor = new SingleLevelVisitor(typeof(EntityComponent), true);

                    DataVisitNodeBuilder.Run(TypeDescriptorFactory.Default, newEntity, new List<IDataCustomVisitor>()
                    {
                        entityVisitor,
                        entityComponentVisitor
                    });


                    foreach (var entityToRemove in entitiesToRemoveFromNew)
                    {
                        List<DataVisitNode> nodes;
                        if (entityVisitor.References.TryGetValue(entityToRemove.Id, out nodes))
                        {
                            foreach (var node in nodes)
                            {
                                node.RemoveValue();
                            }
                        }
                    }
                }
            }

            // TODO: Merge entity hierarchy

            return result;
        }

        private void MapEntities(EntityHierarchyData hierarchyData, Dictionary<Guid, EntityRemapEntry> entities)
        {
            if (hierarchyData == null)
            {
                return;
            }

            foreach (var entity in hierarchyData.Entities)
            {
                if (entities.ContainsKey(entity.Entity.Id))
                {
                    continue;
                }

                var remap = new EntityRemapEntry(entity);
                // We are removing children from transform component for the diff
                remap.PushChildren();
                entities[entity.Entity.Id] = remap;
            }
        }

        private class EntityRemapEntry
        {
            public EntityRemapEntry(EntityDesign entityDesign)
            {
                if (entityDesign == null) throw new ArgumentNullException(nameof(entityDesign));
                EntityDesign = entityDesign;
            }

            public readonly EntityDesign EntityDesign;

            public List<TransformComponent> Children;

            public void PushChildren()
            {
                foreach (var child in EntityDesign.Entity.Transform.Children)
                {
                    if (Children == null)
                    {
                        Children = new List<TransformComponent>();
                    }
                    Children.Add(child);
                }
                EntityDesign.Entity.Transform.Children.Clear();
            }

            public void PopChildren()
            {
                if (Children != null)
                {
                    EntityDesign.Entity.Transform.Children.AddRange(Children);
                }
            }
        }

        private class SingleLevelVisitor : IDataCustomVisitor
        {
            private readonly Type rootType;
            private readonly bool recordReferences;
            private int level;

            public SingleLevelVisitor(Type type, bool recordReferences)
            {
                rootType = type;
                this.recordReferences = recordReferences;
                References = new Dictionary<Guid, List<DataVisitNode>>();
            }

            public bool CanVisit(Type type)
            {
                return rootType.IsAssignableFrom(type);
            }

            public Dictionary<Guid, List<DataVisitNode>> References { get; }

            public void Reset()
            {
                level = 0;
                References.Clear();
            }

            public void Visit(ref VisitorContext context)
            {
                // Record reference of the instance being visited
                var nodeBuilder = (DataVisitNodeBuilder)context.Visitor;

                if (level == 0)
                {
                    level++;
                    context.Visitor.VisitObject(context.Instance, context.Descriptor, true);
                    level--;
                }
                else if (recordReferences)
                {
                    // Save Entity reference
                    var entity = context.Instance as Entity;
                    var entityComponent = context.Instance as EntityComponent;
                    Guid? id = entity?.Id ?? entityComponent?.Entity.Id;
                    if (id.HasValue)
                    {
                        List<DataVisitNode> nodes;
                        if (!References.TryGetValue(id.Value, out nodes))
                        {
                            nodes = new List<DataVisitNode>();
                            References.Add(id.Value, nodes);
                        }
                        nodes.Add(nodeBuilder.CurrentNode);
                    }
                }
            }
        }
    }
}