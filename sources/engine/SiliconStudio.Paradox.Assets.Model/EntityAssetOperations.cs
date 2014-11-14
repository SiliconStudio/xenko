// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Paradox.Assets.Model.Analysis;
using SiliconStudio.Paradox.Data;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Data;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.EntityModel.Data;

namespace SiliconStudio.Paradox.Assets.Model
{
    public static class EntityAssetOperations
    {
        public static EntityAsset ExtractSceneClone(EntityAsset source, Guid sourceRootEntity)
        {
            if (source == null) throw new ArgumentNullException("source");

            if (source.Hierarchy.RootEntity != sourceRootEntity)
                throw new NotImplementedException("Currently, only cloning a root entity is supported.");

            return (EntityAsset)AssetCloner.Clone(source);
        }

        static IEnumerable<EntityData> EnumerateChildren(this EntityData entity)
        {
            EntityComponentData entityComponent;
            if (!entity.Components.TryGetValue(TransformationComponent.Key, out entityComponent))
                yield break;


            var transformationComponent = (TransformationComponentData)entityComponent;
            foreach (var child in transformationComponent.Children)
            {
                yield return child.Entity.Value;
            }
        }

        static Dictionary<Guid, Guid> MatchTree(EntityAsset tree1, EntityAsset tree2)
        {
            var result = new Dictionary<Guid, Guid>();

            var entities1 = tree1.Hierarchy.Entities;
            var entities2 = tree2.Hierarchy.Entities;
 
            // Try to check if some IDs are matching
            foreach (var commonEntityId in entities1.Select(x => x.Id).Intersect(entities2.Select(x => x.Id)))
                result.Add(commonEntityId, commonEntityId);

            if (result.Count == 0)
            {
                // No ID matched (reimporting from a raw asset without ID?), we have to do name and tree matching
                // For now, we will implement a very simple tree matching that expects same hierarchy/names (using current diff engine)
                // Later, we could check into implementing more advanced techniques,
                // such as http://hci.stanford.edu/publications/2011/Bricolage/FTM-IJCAI2011.pdf
                
            }
            throw new NotImplementedException();
       }

        public static void ImportScene(EntityAsset source, Guid sourceRootEntity, EntityAsset dest, Guid destRootEntityId)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");

            // Extract the scene starting from given root
            // Note: only extracting root is supported as of now
            var clonedSource = ExtractSceneClone(source, sourceRootEntity);

            var newAsset = (EntityAsset)AssetCloner.Clone(clonedSource);

            // Generate entity mapping
            var entityMapping = new Dictionary<Guid, Guid>();
            var reverseEntityMapping = new Dictionary<Guid, Guid>();
            foreach (var entity in newAsset.Hierarchy.Entities)
            {
                // Generate new Id
                var newEntityId = Guid.NewGuid();

                // Update mappings
                entityMapping.Add(newEntityId, entity.Id);
                reverseEntityMapping.Add(entity.Id, newEntityId);

                // Update entity with new id
                entity.Id = newEntityId;
            }

            // Rewrite entity references
            // Should we nullify invalid references?
            var entityAnalysisResult = EntityAnalysis.Visit(newAsset.Hierarchy);
            foreach (var entityReference in entityAnalysisResult.EntityReferences)
            {
                Guid newEntityId;
                reverseEntityMapping.TryGetValue(entityReference.Id, out newEntityId);
                entityReference.Id = newEntityId;
            }

            // Insert those entities
            foreach (var entity in newAsset.Hierarchy.Entities)
            {
                dest.Hierarchy.Entities.Add(entity);
            }

            // Find new root
            var newClonedRoot = newAsset.Hierarchy.Entities[reverseEntityMapping[sourceRootEntity]];

            // Find destination target parent
            var destRootEntity = dest.Hierarchy.Entities[destRootEntityId];
            var destRootTransformation = (TransformationComponentData)destRootEntity.Components[TransformationComponent.Key];

            // Attach new root to target parent
            destRootTransformation.Children.Add(EntityComponentReference.New<TransformationComponent>(newClonedRoot.Components[TransformationComponent.Key]));

            // Add asset base
            // TODO: Use real import asset location?
            //dest.AssetBases.Add(newClonedRoot.Id, new EntityBase { Base = new AssetBase(clonedSource), IdMapping = entityMapping });
        }
    }
}