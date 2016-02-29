// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public static class PrefabAssetOperations
    {
        public static PrefabAssetBase ExtractSceneClone(PrefabAssetBase source, Guid sourceRootEntity)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            // Note: Instead of copying the whole asset (with its potentially big hierarchy), we first copy the asset only (without the hierarchy), then the sub-hierarchy to extract.

            // create the hierarchy of the sub-tree
            var subTreeRoot = source.Hierarchy.Entities[sourceRootEntity].Entity;
            var subTreeHierarchy = new EntityHierarchyData { Entities = { subTreeRoot }, RootEntities = { sourceRootEntity } };
            foreach (var subTreeEntity in subTreeRoot.EnumerateChildren(true))
                subTreeHierarchy.Entities.Add(source.Hierarchy.Entities[subTreeEntity.Id]);

            // clone the entities of the sub-tree
            var clonedHierarchy = (EntityHierarchyData)AssetCloner.Clone(subTreeHierarchy);
            clonedHierarchy.Entities[sourceRootEntity].Entity.Transform.Parent = null;

            // set to null reference outside of the sub-tree
            EntityAnalysis.FixupEntityReferences(clonedHierarchy);

            // temporary nullify the hierarchy to avoid to clone it
            var sourceHierarchy = source.Hierarchy;
            source.Hierarchy = null;

            // clone asset without hierarchy
            var clonedAsset = (PrefabAssetBase)AssetCloner.Clone(source);
            clonedAsset.Hierarchy = clonedHierarchy;

            // revert the source hierarchy
            source.Hierarchy = sourceHierarchy;

            return clonedAsset;
        }

        static IEnumerable<Entity> EnumerateChildren(this Entity entity, bool isRecursive)
        {
            var transformationComponent = entity.Transform;
            if (transformationComponent == null)
                yield break;
            
            foreach (var child in transformationComponent.Children)
            {
                yield return child.Entity;

                if (isRecursive)
                {
                    foreach (var childChild in child.Entity.EnumerateChildren(true))
                    {
                        yield return childChild;
                    }
                }
            }
        }

        public static EntityHierarchyData ImportScene(PrefabAssetBase source, Guid sourceRootEntity)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            // Extract the scene starting from given root
            var newAsset = ExtractSceneClone(source, sourceRootEntity);

            // Generate entity mapping
            var reverseEntityMapping = new Dictionary<Guid, Guid>();
            foreach (var entityDesign in newAsset.Hierarchy.Entities)
            {
                // Generate new Id
                var newEntityId = Guid.NewGuid();

                // Update mappings
                reverseEntityMapping.Add(entityDesign.Entity.Id, newEntityId);

                // Update entity with new id
                entityDesign.Entity.Id = newEntityId;
            }

            // Rewrite entity references
            // Should we nullify invalid references?
            EntityAnalysis.RemapEntitiesId(newAsset.Hierarchy, reverseEntityMapping);

            return newAsset.Hierarchy;
        }
    }
}