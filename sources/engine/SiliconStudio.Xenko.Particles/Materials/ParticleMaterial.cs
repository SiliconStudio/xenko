// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Internals;
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
        /// <summary>
        /// Shader parameters collection for the effect
        /// </summary>
        [DataMemberIgnore]
        private readonly ParameterCollection parameters = new ParameterCollection();

        [DataMemberIgnore]
        private EffectParameterCollectionGroup parameterCollectionGroup;

        [DataMemberIgnore]
        protected List<ParameterCollection> ParameterCollections;

        [DataMemberIgnore]
        protected FastListStruct<ParameterCollection> NewParameterCollections;

        [DataMemberIgnore]
        protected FastListStruct<ParameterCollection> OldParameterCollections;

        [DataMemberIgnore]
        private int effectCompilationAttemptCountdown = 0;

        [DataMemberIgnore]
        private Effect effect = null;

        [DataMemberIgnore]
        private DefaultEffectInstance effectInstance;

        [DataMemberIgnore]
        private DynamicEffectCompiler effectCompiler;

        /// <summary>
        /// Gets or sets the current <see cref="SiliconStudio.Xenko.Graphics.Effect"/> for this material
        /// </summary>
        [DataMemberIgnore]
        public Effect Effect => effect;

        /// <summary>
        /// True if <see cref="InitializeCore"/> has been called
        /// </summary>
        [DataMemberIgnore]
        protected bool IsInitialized { get; private set; } = false;

        /// <summary>
        /// Indicates if the vertex layout required by this material has changed since the last time <see cref="UpdateVertexBuilder"/> was called
        /// </summary>
        [DataMemberIgnore]
        public bool VertexLayoutHasChanged { get; protected set; } = true;

        /// <summary>
        /// Sets the name of the effect or shader which the material will use
        /// </summary>
        [DataMemberIgnore]
        protected abstract string EffectName { get; set; }

        /// <summary>
        /// Gets the <see cref="EffectInputSignature"/> of the <see cref="SiliconStudio.Xenko.Graphics.Effect"/> used by this material
        /// </summary>
        /// <returns></returns>
        public EffectInputSignature GetInputSignature()
        {
            return Effect?.InputSignature;
        }

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
            VertexLayoutHasChanged = false;
        }


        /// <summary>
        /// Setups the current material using the graphics device.
        /// </summary>
        /// <param name="graphicsDevice">Graphics device to setup</param>
        /// <param name="viewMatrix">The camera's View matrix</param>
        /// <param name="projMatrix">The camera's Projection matrix</param>
        public virtual void Setup(GraphicsDevice graphicsDevice, RenderContext context, Matrix viewMatrix, Matrix projMatrix, Color4 color)
        {
            if (!IsInitialized)
            {
                InitializeCore(context);
                IsInitialized = true;
            }          
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

        // TODO Part of the graphics improvement XK-3052
        //  Do not use the RenderContext

        /// <summary>
        /// Initializes the core of the material, such as the shader generator and the parameter collection
        /// </summary>
        /// <param name="context">The current <see cref="RenderContext"/></param>
        protected virtual void InitializeCore(RenderContext context)
        {
            if (EffectName == null) throw new ArgumentNullException("No EffectName specified");

            ParameterCollections = new List<ParameterCollection> { parameters };

            NewParameterCollections = new FastListStruct<ParameterCollection>(8);
            OldParameterCollections = new FastListStruct<ParameterCollection>(8);

            // Setup the effect compiler
            effectInstance = new DefaultEffectInstance(ParameterCollections); // Remove this
            effectCompiler = new DynamicEffectCompiler(context.Services, EffectName, -1); // Image effects are compiled with higher priority
        }

        /// <summary>
        /// Sets a struct value for the specified key in the <see cref="ParameterCollection"/>
        /// </summary>
        /// <typeparam name="T">A valuetype</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SetParameter<T>(ParameterKey<T> key, T value) => parameters.Set(key, value);

        /// <summary>
        /// Updates the material's <see cref="SiliconStudio.Xenko.Graphics.Effect"/> and applies it to the <see cref="GraphicsDevice"/> for rendering
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> used for rendering</param>
        public void ApplyEffect(GraphicsDevice graphicsDevice)
        {
            UpdateEffect(graphicsDevice);

            effect.Apply(graphicsDevice, parameterCollectionGroup, applyEffectStates: false);
        }

        /// <summary>
        /// Updates the dynamic effect if the shader code has changed
        /// </summary>
        /// <param name="graphicsDevice">The current <see cref="GraphicsDevice"/></param>
        private void UpdateEffect(GraphicsDevice graphicsDevice)
        {
            if (effectCompilationAttemptCountdown > 0)
            {
                effectCompilationAttemptCountdown--;
                return;
            }

            try
            {
                effectCompiler.Update(effectInstance, null);
            }
            catch (Exception)
            {
                // If the compilation fails do not update the current effect and try again later
                effectCompilationAttemptCountdown = 30; // Try again in 30 frames
                VertexLayoutHasChanged = false;
                return;
            }

            effect = effectInstance.Effect;

            NewParameterCollections.Clear();
            NewParameterCollections.AddRange(ParameterCollections.ToArray());

            // Get or create parameter collection
            if (parameterCollectionGroup == null || parameterCollectionGroup.Effect != effect || !ArrayExtensions.ArraysReferenceEqual(ref OldParameterCollections, ref NewParameterCollections))
            {
                // It is quite inefficient if user is often switching effect without providing a matching ParameterCollectionGroup
                parameterCollectionGroup = new EffectParameterCollectionGroup(graphicsDevice, effect, ParameterCollections.ToArray());

                OldParameterCollections.Clear();
                OldParameterCollections.AddRange(NewParameterCollections);
            }
        }

    }
}
