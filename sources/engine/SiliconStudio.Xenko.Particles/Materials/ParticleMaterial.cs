// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Particles.Sorters;
using SiliconStudio.Xenko.Particles.VertexLayouts;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles.Materials
{
    /// <summary>
    /// Base class for the particle materials which uses a dynamic effect compiler to generate shaders at runtime
    /// </summary>
    [DataContract("ParticleMaterial")]
    public abstract class ParticleMaterial
    {
        [DataMemberIgnore]
        public readonly ParameterCollection Parameters = new ParameterCollection();

        /// <summary>
        /// True if <see cref="InitializeCore"/> has been called
        /// </summary>
        [DataMemberIgnore]
        protected bool IsInitialized { get; private set; }

        /// <summary>
        /// Indicates if the vertex layout required by this material has changed since the last time <see cref="UpdateVertexBuilder"/> was called
        /// </summary>
        [DataMemberIgnore]
        public bool HasVertexLayoutChanged { get; protected set; } = true;

        /// <summary>
        /// Sets the name of the effect or shader which the material will use
        /// </summary>
        [DataMemberIgnore]
        public abstract string EffectName { get; protected set; }

        /// <summary>
        /// Prepares the material for drawing the current frame with the current <see cref="ParticleVertexBuilder"/> and <see cref="ParticleSorter"/>
        /// </summary>
        /// <param name="vertexBuilder">Current <see cref="ParticleVertexBuilder"/></param>
        /// <param name="sorter">Current <see cref="ParticleSorter"/></param>
        public virtual void PrepareForDraw(ParticleVertexBuilder vertexBuilder, ParticleSorter sorter)
        {
        }

        /// <summary>
        /// Updates the required fields for this frame in the vertex buffer builder.
        /// If nothing has changed since the last frame and the vertex layout is the same, do not add any new required fields
        /// </summary>
        /// <param name="vertexBuilder">The target vertex buffer builder</param>
        public virtual void UpdateVertexBuilder(ParticleVertexBuilder vertexBuilder)
        {
            HasVertexLayoutChanged = false;
        }

        public virtual void ValidateEffect(RenderContext context, ref EffectValidator effectValidator)
        {
        }

        /// <summary>
        /// Setups the current material using the graphics device.
        /// </summary>
        /// <param name="graphicsDevice">Graphics device to setup</param>
        /// <param name="viewMatrix">The camera's View matrix</param>
        /// <param name="projMatrix">The camera's Projection matrix</param>
        public virtual void Setup(RenderContext context)
        {
            if (!IsInitialized)
            {
                InitializeCore(context);
                IsInitialized = true;
            }          
        }

        /// <summary>
        /// Setup the pipeline state object.
        /// </summary>
        /// <param name="renderContext"></param>
        /// <param name="pipelineState"></param>
        public virtual void SetupPipeline(RenderContext renderContext, PipelineStateDescription pipelineState)
        {
        }

        /// <summary>
        /// Patch the particle's vertex buffer which was already built by the <see cref="ShapeBuilders.ShapeBuilder"/>
        /// This involes animating hte uv coordinates and filling per-particle fields, such as the color field
        /// </summary>
        /// <param name="vertexBuilder">The target buffer builder to use for patching the vertex data</param>
        /// <param name="invViewX">Unit vector X (right) in camera space, extracted from the inverse view matrix</param>
        /// <param name="invViewY">Unit vector Y (up) in camera space, extracted from the inverse view matrix</param>
        /// <param name="sorter">Particle enumerator which can be iterated and returns sported particles</param>
        public virtual void PatchVertexBuffer(ParticleVertexBuilder vertexBuilder, Vector3 invViewX, Vector3 invViewY, ParticleSorter sorter)
        {
        }

        /// <summary>
        /// Initializes the core of the material, such as the shader generator and the parameter collection
        /// </summary>
        /// <param name="context">The current <see cref="RenderContext"/></param>
        protected virtual void InitializeCore(RenderContext context)
        {
        }
    }
}
