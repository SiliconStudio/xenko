// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Paradox.Engine.Graphics.Composers;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Instantiation of a <see cref="Model"/> through a <see cref="RenderPipeline"/>.
    /// </summary>
    public sealed class RenderModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderModel" /> class.
        /// </summary>
        /// <param name="sceneRenderer">The scene renderer.</param>
        /// <param name="modelInstance">The model instance.</param>
        /// <exception cref="System.ArgumentNullException">pipeline</exception>
        public RenderModel(SceneRenderer sceneRenderer, IModelInstance modelInstance)
        {
            if (sceneRenderer == null) throw new ArgumentNullException("sceneRenderer");

            SceneRenderer = sceneRenderer;
            ModelInstance = modelInstance;
            Model = modelInstance.Model;
            Parameters = modelInstance.Parameters;

            var modelRendererState = sceneRenderer.GetOrCreateModelRendererState();
            RenderMeshes = new List<RenderMesh>[modelRendererState.ModelSlotMapping.Count];

            if (Model != null)
            {
                foreach (var modelSlot in modelRendererState.ModelSlotMapping)
                {
                    modelSlot.Value.PrepareRenderModel(this);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderModel" /> class.
        /// </summary>
        /// <param name="sceneRenderer">The scene renderer.</param>
        /// <param name="model">The model.</param>
        /// <exception cref="System.ArgumentNullException">pipeline</exception>
        [Obsolete]
        public RenderModel(SceneRenderer sceneRenderer, Model model)
        {
            if (sceneRenderer == null) throw new ArgumentNullException("pipeline");
            SceneRenderer = sceneRenderer;
            Model = model;
            var slotCount = sceneRenderer.GetOrCreateModelRendererState().ModelSlotMapping.Count;
            RenderMeshes = new List<RenderMesh>[slotCount];
        }

        /// <summary>
        /// Gets the meshes instantiated for this view.
        /// </summary>
        /// <value>
        /// The meshes instantiated for this view.
        /// </value>
        public FastListStruct<List<RenderMesh>> RenderMeshes;

        /// <summary>
        /// The model instance
        /// </summary>
        public readonly IModelInstance ModelInstance;

        /// <summary>
        /// Gets the underlying model.
        /// </summary>
        /// <value>
        /// The underlying model.
        /// </value>
        public readonly Model Model;

        /// <summary>
        /// Gets the instance parameters to this model.
        /// </summary>
        /// <value>
        /// The instance parameters to this model.
        /// </value>
        public readonly ParameterCollection Parameters;

        /// <summary>
        /// Gets the render pipeline.
        /// </summary>
        /// <value>
        /// The render pipeline.
        /// </value>
        public readonly SceneRenderer SceneRenderer;

        public Material GetMaterial(int materialIndex)
        {
            // TBD, but for now, -1 means null material
            if (materialIndex == -1)
                return null;

            // Try to get material first from model instance, then model
            return GetMaterialHelper(ModelInstance.Materials, materialIndex)
                   ?? GetMaterialHelper(Model.Materials, materialIndex);
        }

        private static Material GetMaterialHelper(List<Material> materials, int index)
        {
            if (materials != null && index < materials.Count)
            {
                return materials[index];
            }

            return null;
        }
    }
}