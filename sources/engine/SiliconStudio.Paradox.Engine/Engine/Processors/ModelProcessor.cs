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
    public class ModelProcessor : EntityProcessor<ModelProcessor.RenderModelItem>
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

        protected override RenderModelItem GenerateAssociatedData(Entity entity)
        {
            return new RenderModelItem(new RenderModel(entity.Get<ModelComponent>()), entity.Transform);
        }

        public Dictionary<Entity, RenderModelItem> EntityToRenderModel
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

        /// <summary>
        /// Queries the list of <see cref="RenderModelCollection"/> for the specified mask
        /// </summary>
        /// <param name="mask">The mask.</param>
        /// <param name="outputCollection">The output collection.</param>
        public void QueryModelGroupsByMask(EntityGroupMask mask, List<RenderModelCollection> outputCollection)
        {
            // Get all meshes from the render model processor
            foreach (var renderModelGroup in ModelGroups)
            {
                // Perform culling on group and accept
                if (!mask.Contains(renderModelGroup.Group))
                {
                    continue;
                }
                outputCollection.Add(renderModelGroup);
            }
        }

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

            RenderModelItem modelEntityData;
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
                var item = matchingEntity.Value;
                var renderModel = item.RenderModel;

                // Skip disabled model components, or model components without a proper model set
                if (!renderModel.Update())
                {
                    continue;
                }

                // Add the render model to the specified collection group
                var groupIndex = (int)matchingEntity.Key.Group;
                groupMaskUsed |= (EntityGroupMask)(1 << groupIndex);
                var modelCollection = allModelGroups[groupIndex];
                modelCollection.Add(renderModel);

                Vector3 scale, translation;
                Matrix rotation;

                bool isScalingNegative = false;
                if (item.TransformComponent.WorldMatrix.Decompose(out scale, out rotation, out translation))
                    isScalingNegative = scale.X * scale.Y * scale.Z < 0.0f;
                item.ModelComponent.Update(ref item.TransformComponent.WorldMatrix, isScalingNegative);

                if (item.Links != null)
                {
                    var modelViewHierarchy = item.ModelComponent.ModelViewHierarchy;

                    // Update links: transfer node/bone transformation to a specific entity transformation
                    // Then update this entity transformation tree
                    // TODO: Ideally, we should order update (matchingEntities?) to avoid updating a ModelViewHierarchy before its transformation is updated.
                    foreach (var link in item.Links)
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

        public class RenderModelItem
        {
            public RenderModelItem(RenderModel renderModel, TransformComponent transformComponent)
            {
                RenderModel = renderModel;
                ModelComponent = renderModel.ModelComponent;
                TransformComponent = transformComponent;
            }

            public readonly RenderModel RenderModel;

            public readonly ModelComponent ModelComponent;

            public readonly TransformComponent TransformComponent;

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