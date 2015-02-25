// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine.Graphics;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// The processor for <see cref="ModelComponent"/>.
    /// </summary>
    public class ModelProcessor : EntityProcessor<RenderModel>
    {
        /// <summary>
        /// The link transformation to update.
        /// </summary>
        /// <remarks>The collection is declared globally only to avoid allocation at each frames</remarks>
        private readonly FastCollection<TransformComponent> linkTransformationToUpdate = new FastCollection<TransformComponent>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelProcessor"/> class.
        /// </summary>
        public ModelProcessor()
            : base(new PropertyKey[] { ModelComponent.Key, TransformComponent.Key })
        {
            Models = new List<RenderModel>();
        }

        protected override RenderModel GenerateAssociatedData(Entity entity)
        {
            return new RenderModel(entity);
        }

        /// <summary>
        /// Gets the current models to render.
        /// </summary>
        /// <value>The current models to render.</value>
        public List<RenderModel> Models { get; private set; }

        public EntityLink LinkEntity(Entity linkedEntity, ModelComponent modelComponent, string boneName)
        {
            var modelEntityData = matchingEntities[modelComponent.Entity];
            var nodeIndex = modelEntityData.ModelComponent.ModelViewHierarchy.Nodes.IndexOf(x => x.Name == boneName);

            var entityLink = new EntityLink { Entity = linkedEntity, ModelComponent = modelComponent, NodeIndex = nodeIndex };
            if (nodeIndex == -1)
                return entityLink;

            linkedEntity.Transform.isSpecialRoot = true;
            linkedEntity.Transform.UseTRS = false;

            if (modelEntityData.Links == null)
                modelEntityData.Links = new List<EntityLink>();

            modelEntityData.Links.Add(entityLink);

            return entityLink;
        }

        public bool UnlinkEntity(EntityLink entityLink)
        {
            if (entityLink.NodeIndex == -1)
                return false;

            RenderModel modelEntityData;
            if (!matchingEntities.TryGetValue(entityLink.ModelComponent.Entity, out modelEntityData))
                return false;

            return modelEntityData.Links.Remove(entityLink);
        }

        public override void Draw(RenderContext context)
        {
            Models.Clear();

            // Collect models for this frame
            foreach (var matchingEntity in enabledEntities)
            {
                var renderModel = matchingEntity.Value;

                // Skip disabled model components, or model components without a proper model set
                if (!renderModel.ModelComponent.Enabled || renderModel.ModelComponent.ModelViewHierarchy == null || renderModel.ModelComponent.Model == null)
                {
                    continue;
                }

                // Update the group in case it changed
                renderModel.Update();

                Models.Add(renderModel);

                var modelViewHierarchy = renderModel.ModelComponent.ModelViewHierarchy;
                var transformationComponent = renderModel.TransformComponent;

                var links = renderModel.Links;

                // Update model view hierarchy node matrices
                modelViewHierarchy.NodeTransformations[0].LocalMatrix = transformationComponent.WorldMatrix;
                modelViewHierarchy.UpdateMatrices();

                if (links != null)
                {
                    // Update links: transfer node/bone transformation to a specific entity transformation
                    // Then update this entity transformation tree
                    // TODO: Ideally, we should order update (matchingEntities?) to avoid updating a ModelViewHierarchy before its transformation is updated.
                    foreach (var link in renderModel.Links)
                    {
                        var linkTransformation = link.Entity.Transform;
                        linkTransformation.LocalMatrix = modelViewHierarchy.NodeTransformations[link.NodeIndex].WorldMatrix;

                        linkTransformationToUpdate.Clear();
                        linkTransformationToUpdate.Add(linkTransformation);
                        TransformProcessor.UpdateTransformations(linkTransformationToUpdate, false);
                    }
                }

                // Upload matrices to TransformationKeys.World
                modelViewHierarchy.UpdateToRenderModel(renderModel);

                // Upload skinning blend matrices
                MeshSkinningUpdater.Update(modelViewHierarchy, renderModel);
            }
        }

        
        public struct EntityLink
        {
            public int NodeIndex;
            public Entity Entity;
            public ModelComponent ModelComponent;
        }
    }
}