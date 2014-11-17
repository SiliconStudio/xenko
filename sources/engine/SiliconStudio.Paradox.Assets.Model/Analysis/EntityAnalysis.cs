﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Data;

namespace SiliconStudio.Paradox.Assets.Model.Analysis
{
    public static class EntityAnalysis
    {
        public struct Result
        {
            public List<IEntityComponentReference> EntityComponentReferences;
            public List<EntityReference> EntityReferences;
        }

        public static Result Visit(EntityAsset entityAsset)
        {
            if (entityAsset == null) throw new ArgumentNullException("obj");

            var entityReferenceVistor = new EntityReferenceAnalysis();
            entityReferenceVistor.Visit(entityAsset);

            return entityReferenceVistor.Result;
        }

        /// <summary>
        /// Updates <see cref="EntityReference.Id"/>, <see cref="EntityReference.Name"/>, <see cref="EntityComponentReference{T}.Entity"/>
        /// and <see cref="EntityComponentReference{T}.Component"/>, while also checking integrity of given <see cref="EntityAsset"/>.
        /// </summary>
        /// <param name="entityAsset">The entity asset.</param>
        public static void UpdateEntityReferences(EntityAsset entityAsset)
        {
            var entityAnalysisResult = Visit(entityAsset);

            // Updates EntityComponent references
            foreach (var entityComponentReference in entityAnalysisResult.EntityComponentReferences)
            {
                if (entityComponentReference.Value != null)
                {
                    var containingEntity = entityComponentReference.Value.Entity;
                    if (containingEntity == null)
                        throw new InvalidOperationException("Found a reference to a component which doesn't have any entity");

                    // If we have a component value but no entity, update it
                    if (entityComponentReference.Entity.Value == null)
                        entityComponentReference.Entity.Value = containingEntity;
                    //else if (entityComponentReference.Entity.Value != containingEntity)
                    //    throw new InvalidOperationException("Entity reference doesn't seem to match Component actual entity");

                    // If we have a component value but no component key, try to find ourself in containing entity
                    if (entityComponentReference.Component == null)
                    {
                        foreach (var component in containingEntity.Components)
                        {
                            if (component.Value == entityComponentReference.Value)
                            {
                                entityComponentReference.Component = component.Key;
                                break;
                            }
                        }
                        if (entityComponentReference.Component == null)
                        {
                            throw new InvalidOperationException("Could not find a component in its containing Entity");
                        }
                    }

                    // Make sure this component belongs to this container
                    if (entityComponentReference.Value.Entity.Container != entityAsset.Hierarchy)
                    {
                        throw new InvalidOperationException("It seems this component and/or entity doesn't belong to this asset");
                    }
                }
            }

            // Updates Entity references
            foreach (var entityReference in entityAnalysisResult.EntityReferences)
            {
                if (entityReference.Value != null)
                {
                    entityReference.Id = entityReference.Value.Id;
                    entityReference.Name = entityReference.Value.Name;
                }
            }
        }

        private class EntityReferenceAnalysis : AssetVisitorBase
        {
            public EntityReferenceAnalysis()
            {
                var result = new Result();
                result.EntityComponentReferences = new List<IEntityComponentReference>();
                result.EntityReferences = new List<EntityReference>();
                Result = result;
            }

            public Result Result { get; private set; }

            public override void VisitObject(object obj, ObjectDescriptor descriptor, bool visitMembers)
            {
                base.VisitObject(obj, descriptor, visitMembers);
                var entityReference = obj as EntityReference;
                if (entityReference != null)
                {
                    Result.EntityReferences.Add(entityReference);
                }

                var componentReference = obj as IEntityComponentReference;
                if (componentReference != null)
                {
                    Result.EntityComponentReferences.Add(componentReference);
                }
            }
        }
    }
}