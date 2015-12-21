// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Internals;
using SiliconStudio.Xenko.Particles.Sorters;
using SiliconStudio.Xenko.Particles.VertexLayouts;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Particles.Materials
{
    [DataContract("ParticleMaterialBase")]
    public abstract class ParticleMaterialBase
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
        protected FastListStruct<ParameterCollection> newParameterCollections;

        [DataMemberIgnore]
        protected FastListStruct<ParameterCollection> oldParameterCollections;

        /// <summary>
        /// Sets the name of the effect or shader which the material will use
        /// </summary>
        [DataMemberIgnore]
        protected abstract string EffectName { get; set; }

        [DataMemberIgnore]
        private Effect effect = null;

        [DataMemberIgnore]
        public Effect Effect => effect;

        [DataMemberIgnore]
        private DefaultEffectInstance effectInstance;

        [DataMemberIgnore]
        private DynamicEffectCompiler effectCompiler;

        public EffectInputSignature GetInputSignature()
        {
            return Effect?.InputSignature;
        }

        /// <summary>
        /// True if <see cref="ParticleMaterialBase.Initialize"/> has been called
        /// </summary>
        [DataMemberIgnore]
        protected bool isInitialized = false;

        [DataMemberIgnore]
        public bool VertexLayoutHasChanged { get; protected set; } = true;

        public virtual void PrepareForDraw(ParticleVertexBuilder vertexBuilder, ParticleSorter sorter)
        {
        }

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
            if (!isInitialized)
            {
                InitializeCore(context);
                isInitialized = true;
            }          
        }

        public virtual void PatchVertexBuffer(ParticleVertexBuilder vertexBuilder, Vector3 invViewX, Vector3 invViewY, ParticleSorter sorter)
        {
        }

        protected virtual void InitializeCore(RenderContext context)
        {
            if (EffectName == null) throw new ArgumentNullException("No EffectName specified");

            ParameterCollections = new List<ParameterCollection> { parameters };

            newParameterCollections = new FastListStruct<ParameterCollection>(8);
            oldParameterCollections = new FastListStruct<ParameterCollection>(8);

            // Setup the effect compiler
            effectInstance = new DefaultEffectInstance(ParameterCollections); // Remove this
            effectCompiler = new DynamicEffectCompiler(context.Services, EffectName, -1); // Image effects are compiled with higher priority
        }

        public void SetParameter<T>(ParameterKey<T> key, T value) => parameters.Set(key, value);


        public void ApplyEffect(GraphicsDevice graphicsDevice)
        {
            UpdateEffect(graphicsDevice);

            effect.Apply(graphicsDevice, parameterCollectionGroup, applyEffectStates: false);
        }

        private void UpdateEffect(GraphicsDevice graphicsDevice)
        {
            effectCompiler.Update(effectInstance, null);

            effect = effectInstance.Effect;

            newParameterCollections.Clear();
            newParameterCollections.AddRange(ParameterCollections.ToArray());

            // Get or create parameter collection
            if (parameterCollectionGroup == null || parameterCollectionGroup.Effect != effect || !ArrayExtensions.ArraysReferenceEqual(ref oldParameterCollections, ref newParameterCollections))
            {
                // It is quite inefficient if user is often switching effect without providing a matching ParameterCollectionGroup
                parameterCollectionGroup = new EffectParameterCollectionGroup(graphicsDevice, effect, ParameterCollections.ToArray());

                oldParameterCollections.Clear();
                oldParameterCollections.AddRange(newParameterCollections);
            }
        }

    }
}
