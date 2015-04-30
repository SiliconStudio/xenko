// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Engine.Processors
{
    /// <summary>
    /// The processor for <see cref="ModelComponent"/>.
    /// </summary>
    public class ModelProcessor : EntityProcessor<RenderModel>
    {
        // TODO: ModelProcessor should be decoupled from RenderModel

        private readonly RenderModelCollection[] allModelGroups; 

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
            ModelGroups = new List<RenderModelCollection>();
            allModelGroups = new RenderModelCollection[32];
            for (int i = 0; i < allModelGroups.Length; i++)
            {
                allModelGroups[i] = new RenderModelCollection((EntityGroup)i);
            }
        }

        protected override RenderModel GenerateAssociatedData(Entity entity)
        {
            return new RenderModel(entity);
        }

        public Dictionary<Entity, RenderModel> EntityToRenderModel
        {
            get
            {
                return enabledEntities;
            }
        }

        /// <summary>
        /// Gets the current models to render per group.
        /// </summary>
        /// <value>The current models to render.</value>
        public List<RenderModelCollection> ModelGroups { get; private set; }

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
            // Clear previous model groups
            foreach (var modelGroup in ModelGroups)
            {
                modelGroup.Clear();
            }
            ModelGroups.Clear();

            var groupMaskUsed = EntityGroupMask.None;

            // Collect models for this frame, and dispatch them to list of group
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

                // Add the render model to the specified collection group
                var groupIndex = (int)renderModel.Group;
                groupMaskUsed |= (EntityGroupMask)(1 << groupIndex);
                var modelCollection = allModelGroups[groupIndex];
                modelCollection.Add(renderModel);

                var modelComponent = renderModel.ModelComponent;
                var modelViewHierarchy = renderModel.ModelComponent.ModelViewHierarchy;
                var transformationComponent = renderModel.TransformComponent;

                var links = renderModel.Links;

                modelComponent.Update(ref transformationComponent.WorldMatrix);

                if (links != null)
                {
                    // Update links: transfer node/bone transformation to a specific entity transformation
                    // Then update this entity transformation tree
                    // TODO: Ideally, we should order update (matchingEntities?) to avoid updating a ModelViewHierarchy before its transformation is updated.
                    foreach (var link in renderModel.Links)
                    {
                        var linkTransformation = link.Entity.Transform;
                        Matrix linkedLocalMatrix;
                        TransformComponent.CreateMatrixTRS(ref linkTransformation.Position, ref linkTransformation.Rotation, ref linkTransformation.Scale, out linkedLocalMatrix);
                        Matrix.Multiply(ref linkedLocalMatrix, ref modelViewHierarchy.NodeTransformations[link.NodeIndex].WorldMatrix, out linkTransformation.LocalMatrix);

                        linkTransformationToUpdate.Clear();
                        linkTransformationToUpdate.Add(linkTransformation);
                        TransformProcessor.UpdateTransformations(linkTransformationToUpdate, false);
                    }
                }

                // TODO: World update and skinning is now perform at ModelComponentRenderer time. Check if we can find a better place to do this.

                //// Upload matrices to TransformationKeys.World
                //modelViewHierarchy.UpdateToRenderModel(renderModel);

                //// Upload skinning blend matrices
                //MeshSkinningUpdater.Update(modelViewHierarchy, renderModel);
            }

            // Collect model groups
            for (int groupIndex = 0, groupMask = (int)groupMaskUsed; groupMask != 0; groupMask = groupMask >> 1, groupIndex++)
            {
                if ((groupMask & 1) == 0)
                {
                    continue;
                }

                var modelGroup = allModelGroups[groupIndex];
                ModelGroups.Add(modelGroup);
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