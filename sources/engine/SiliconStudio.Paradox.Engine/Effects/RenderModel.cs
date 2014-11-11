// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Instantiation of a <see cref="Model"/> through a <see cref="RenderPipeline"/>.
    /// </summary>
    public class RenderModel
    {
        private readonly RenderPipeline pipeline;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderModel"/> class.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="model">The model.</param>
        /// <param name="modelComponentParameters">ModelComponent parameters.</param>
        public RenderModel(RenderPipeline pipeline, Model model, ParameterCollection modelComponentParameters = null)
        {
            if (pipeline == null) throw new ArgumentNullException("pipeline");
            if (model == null) throw new ArgumentNullException("model");

            this.pipeline = pipeline;
            Model = model;

            var meshState = pipeline.GetMeshRenderState();
            if (meshState != null)
            {
                InternalMeshes = new List<EffectMesh>[meshState.MeshPassSlotCount];
                meshState.PrepareRenderModel(this, modelComponentParameters);
            }
        }

        /// <summary>
        /// Gets the meshes instantiated for this view.
        /// </summary>
        /// <value>
        /// The meshes instantiated for this view.
        /// </value>
        public List<EffectMesh>[] InternalMeshes { get; private set; }

        /// <summary>
        /// Gets or sets the underlying model.
        /// </summary>
        /// <value>
        /// The underlying model.
        /// </value>
        public Model Model { get; set; }

        /// <summary>
        /// Gets the render pipeline.
        /// </summary>
        /// <value>
        /// The render pipeline.
        /// </value>
        public RenderPipeline Pipeline
        {
            get { return this.pipeline; }
        }
    }
}