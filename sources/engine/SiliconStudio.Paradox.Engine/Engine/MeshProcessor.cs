// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Engine
{
    public class MeshProcessor : EntityProcessor<MeshProcessor.AssociatedData>
    {
        private RenderSystem renderSystem;

        /// <summary>
        /// The link transformation to update.
        /// </summary>
        /// <remarks>The collection is declared globally only to avoid allocation at each frames</remarks>
        private FastCollection<TransformationComponent> linkTransformationToUpdate = new FastCollection<TransformationComponent>();

        public MeshProcessor()
            : base(new PropertyKey[] { ModelComponent.Key, TransformationComponent.Key })
        {
        }

        protected internal override void OnSystemAdd()
        {
            renderSystem = Services.GetSafeServiceAs<RenderSystem>();
        }

        protected override AssociatedData GenerateAssociatedData(Entity entity)
        {
            return new AssociatedData { ModelComponent = entity.Get(ModelComponent.Key), TransformationComponent = entity.Transformation };
        }

        protected override void OnEntityAdding(Entity entity, AssociatedData associatedData)
        {
            associatedData.RenderModels = new List<KeyValuePair<ModelRendererState, RenderModel>>();

            // Initialize a RenderModel for every pipeline
            // TODO: Track added/removed pipelines?
            var modelInstance = associatedData.ModelComponent;

            foreach (var pipeline in renderSystem.Pipelines)
            {
                var modelRenderState = pipeline.GetOrCreateModelRendererState();

                // If the model is not accepted
                if (!modelRenderState.IsValid || !modelRenderState.AcceptModel(modelInstance))
                {
                    continue;
                }

                var renderModel = new RenderModel(pipeline, modelInstance);
                if (renderModel.RenderMeshes == null)
                {
                    continue;
                }
                
                // Register RenderModel
                associatedData.RenderModels.Add(new KeyValuePair<ModelRendererState, RenderModel>(modelRenderState, renderModel));
            }
        }

        protected override void OnEntityRemoved(Entity entity, AssociatedData data)
        {
            base.OnEntityRemoved(entity, data);
        }

        public EntityLink LinkEntity(Entity linkedEntity, ModelComponent modelComponent, string boneName)
        {
            var modelEntityData = matchingEntities[modelComponent.Entity];
            var nodeIndex = modelEntityData.ModelComponent.ModelViewHierarchy.Nodes.IndexOf(x => x.Name == boneName);

            var entityLink = new EntityLink { Entity = linkedEntity, ModelComponent = modelComponent, NodeIndex = nodeIndex };
            if (nodeIndex == -1)
                return entityLink;

            linkedEntity.Transformation.isSpecialRoot = true;
            linkedEntity.Transformation.UseTRS = false;

            if (modelEntityData.Links == null)
                modelEntityData.Links = new List<EntityLink>();

            modelEntityData.Links.Add(entityLink);

            return entityLink;
        }

        public bool UnlinkEntity(EntityLink entityLink)
        {
            if (entityLink.NodeIndex == -1)
                return false;

            AssociatedData modelEntityData;
            if (!matchingEntities.TryGetValue(entityLink.ModelComponent.Entity, out modelEntityData))
                return false;

            return modelEntityData.Links.Remove(entityLink);
        }

        public override void Draw(GameTime time)
        {
            // Clear all pipelines from previously collected models
            foreach (var pipeline in renderSystem.Pipelines)
            {
                var renderMeshState = pipeline.GetOrCreateModelRendererState();
                renderMeshState.RenderModels.Clear();
            }

            // Collect models for this frame
            foreach (var matchingEntity in enabledEntities)
            {
                // Skip model not enabled
                if (!matchingEntity.Value.ModelComponent.Enabled)
                {
                    continue;
                }

                var modelViewHierarchy = matchingEntity.Value.ModelComponent.ModelViewHierarchy;

                var transformationComponent = matchingEntity.Value.TransformationComponent;

                var links = matchingEntity.Value.Links;

                // Update model view hierarchy node matrices
                modelViewHierarchy.NodeTransformations[0].LocalMatrix = transformationComponent.WorldMatrix;
                modelViewHierarchy.UpdateMatrices();

                if (links != null)
                {
                    // Update links: transfer node/bone transformation to a specific entity transformation
                    // Then update this entity transformation tree
                    // TODO: Ideally, we should order update (matchingEntities?) to avoid updating a ModelViewHierarchy before its transformation is updated.
                    foreach (var link in matchingEntity.Value.Links)
                    {
                        var linkTransformation = link.Entity.Transformation;
                        linkTransformation.LocalMatrix = modelViewHierarchy.NodeTransformations[link.NodeIndex].WorldMatrix;

                        linkTransformationToUpdate.Clear();
                        linkTransformationToUpdate.Add(linkTransformation);
                        TransformationProcessor.UpdateTransformations(linkTransformationToUpdate, false);
                    }
                }

                foreach (var renderModelEntry in matchingEntity.Value.RenderModels)
                {
                    var renderModelState = renderModelEntry.Key;
                    var renderModel = renderModelEntry.Value;

                    // Add model to rendering
                    renderModelState.RenderModels.Add(renderModel);

                    // Upload matrices to TransformationKeys.World
                    modelViewHierarchy.UpdateToRenderModel(renderModel);

                    // Upload skinning blend matrices
                    MeshSkinningUpdater.Update(modelViewHierarchy, renderModel);
                }
            }
        }

        public class AssociatedData
        {
            public ModelComponent ModelComponent;

            public TransformationComponent TransformationComponent;

            internal List<KeyValuePair<ModelRendererState, RenderModel>> RenderModels;

            public List<EntityLink> Links;
        }

        public struct EntityLink
        {
            public int NodeIndex;
            public Entity Entity;
            public ModelComponent ModelComponent;
        }
    }
}