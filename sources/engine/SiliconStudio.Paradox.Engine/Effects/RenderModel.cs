// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

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
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="modelInstance">The model instance.</param>
        /// <exception cref="System.ArgumentNullException">pipeline</exception>
        public RenderModel(RenderPipeline pipeline, IModelInstance modelInstance)
        {
            if (pipeline == null) throw new ArgumentNullException("pipeline");

            Pipeline = pipeline;
            ModelInstance = modelInstance;
            Model = modelInstance.Model;
            Parameters = modelInstance.Parameters;

            var modelRendererState = Pipeline.GetOrCreateModelRendererState();
            var slotCount = modelRendererState.ModelSlotCount;
            RenderMeshes = new List<RenderMesh>[slotCount];
            if (Model != null)
            {
                modelRendererState.PrepareRenderModel(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderModel"/> class.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="model">The model.</param>
        /// <exception cref="System.ArgumentNullException">pipeline</exception>
        [Obsolete]
        public RenderModel(RenderPipeline pipeline, Model model)
        {
            if (pipeline == null) throw new ArgumentNullException("pipeline");
            Pipeline = pipeline;
            Model = model;
            var slotCount = Pipeline.GetOrCreateModelRendererState().ModelSlotCount;
            RenderMeshes = new List<RenderMesh>[slotCount];
        }

        /// <summary>
        /// Gets the meshes instantiated for this view.
        /// </summary>
        /// <value>
        /// The meshes instantiated for this view.
        /// </value>
        public readonly List<RenderMesh>[] RenderMeshes;

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
        public readonly RenderPipeline Pipeline;
    }
}