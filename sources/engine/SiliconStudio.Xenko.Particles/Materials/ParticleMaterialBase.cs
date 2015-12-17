// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
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
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Particles.Materials
{
    public enum ParticleMaterialCulling : byte
    {
        CullNone = 0,
        CullBack = 1,
        CullFront = 2
    }

    [DataContract("ParticleMaterialBase")]
    public abstract class ParticleMaterialBase
    {
        // TODO Move to ParticleMaterialSimple and change the name
        [DataMember(20)]
        [DataMemberRange(0, 1, 0.001, 0.1)]
        [Display("Alpha-Additive")]
        public float AlphaAdditive { get; set; } = 0f;

        // TODO Move to ParticleMaterialSimple ? Keep here?
        [DataMember(40)]
        [Display("Face culling")]
        public ParticleMaterialCulling FaceCulling;

        // Parameters should be divided into several groups later.
        // CB0 - Parameters like camera position, viewProjMatrix, Screen size, FOV, etc. which persist for all materials/emitters in the same stage
        // CB1 - Material attributes which persist for all batched together emitters
        // CB2 - (Maybe) Per-emitter attributes.

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

        /// <summary>
        /// True if <see cref="ParticleMaterialBase.Initialize"/> has been called
        /// </summary>
        [DataMemberIgnore]
        protected bool isInitialized = false;


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

            // Setup graphics device - culling, blend states and depth testing

            if (FaceCulling == ParticleMaterialCulling.CullNone) graphicsDevice.SetRasterizerState(graphicsDevice.RasterizerStates.CullNone);
            if (FaceCulling == ParticleMaterialCulling.CullBack) graphicsDevice.SetRasterizerState(graphicsDevice.RasterizerStates.CullBack);
            if (FaceCulling == ParticleMaterialCulling.CullFront) graphicsDevice.SetRasterizerState(graphicsDevice.RasterizerStates.CullFront);

            graphicsDevice.SetBlendState(graphicsDevice.BlendStates.AlphaBlend);

            graphicsDevice.SetDepthStencilState(graphicsDevice.DepthStencilStates.DepthRead);


            // Setup the parameters

            SetParameter(ParticleBaseKeys.ColorIsSRgb, graphicsDevice.ColorSpace == ColorSpace.Linear);

            // This is correct. We invert the value here to reduce calculations on the shader side later
            SetParameter(ParticleBaseKeys.AlphaAdditive, 1f - AlphaAdditive);

            // Scale up the color intensity - might depend on the eye adaptation later
            SetParameter(ParticleBaseKeys.ColorScale, color);

            ///////////////
            // This should be CB0 - view/proj matrices don't change per material
            SetParameter(ParticleBaseKeys.MatrixTransform, viewMatrix * projMatrix);

        }

        public virtual unsafe void PatchVertexBuffer(ParticleVertexBuffer vtxBuilder, Vector3 invViewX, Vector3 invViewY, ParticleSorter sorter)
        {
            var lifeField = sorter.GetField(ParticleFields.RemainingLife);
            var randField = sorter.GetField(ParticleFields.RandomSeed);

            if (!randField.IsValid() || !lifeField.IsValid())
                return;

            var colorField = sorter.GetField(ParticleFields.Color);
            var hasColorField = colorField.IsValid();

            var colAttribute  = vtxBuilder.GetAccessor(VertexAttributes.Color);
            var lifeAttribute = vtxBuilder.GetAccessor(VertexAttributes.Lifetime);
            var randAttribute = vtxBuilder.GetAccessor(VertexAttributes.RandomSeed);

            foreach (var particle in sorter)
            {
                var color = hasColorField ? (uint)(*(Color4*)particle[colorField]).ToRgba() : 0xFFFFFFFF;
                vtxBuilder.SetAttributePerParticle(colAttribute, (IntPtr)(&color));

                vtxBuilder.SetAttributePerParticle(lifeAttribute, particle[lifeField]);

                vtxBuilder.SetAttributePerParticle(randAttribute, particle[randField]);

                vtxBuilder.NextParticle();
            }

            vtxBuilder.RestartBuffer();
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


        protected void ApplyEffect(GraphicsDevice graphicsDevice)
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
