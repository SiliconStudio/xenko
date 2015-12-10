// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Internals;
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
        [DataMember(20)]
        [DataMemberRange(0, 1, 0.001, 0.1)]
        [Display("Emissive power")]
        public float AlphaAdditive { get; set; } = 1f;

        [DataMember(30)]
        [DataMemberRange(0, 100, 0.01, 1)]
        [Display("Intensity")]
        public float ColorIntensity { get; set; } = 1f; // TODO switch to IComputeScalar

        [DataMember(40)]
        [Display("Face culling")]
        public ParticleMaterialCulling FaceCulling;

        [DataMemberIgnore]
        public ParticleEffectVariation MandatoryVariation { get; protected set; } = ParticleEffectVariation.None;

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
        private List<ParameterCollection> parameterCollections;

        /// <summary>
        /// Sets the name of the effect or shader which the material will use
        /// </summary>
        [DataMemberIgnore]
        protected abstract string EffectName { get; set; }

        [DataMemberIgnore]
        private Effect effect = null;

        [DataMemberIgnore]
        private DefaultEffectInstance effectInstance;

        [DataMemberIgnore]
        private DynamicEffectCompiler effectCompiler;

        /// <summary>
        /// True if <see cref="ParticleMaterialBase.InitializeCore"/> has been called
        /// </summary>
        [DataMemberIgnore]
        private bool isInitialized = false;


        /// <summary>
        /// Setups the current material using the graphics device.
        /// </summary>
        /// <param name="graphicsDevice">Graphics device to setup</param>
        /// <param name="viewMatrix">The camera's View matrix</param>
        /// <param name="projMatrix">The camera's Projection matrix</param>
        public virtual void Setup(GraphicsDevice graphicsDevice, RenderContext context, Matrix viewMatrix, Matrix projMatrix, Color4 color)
        {
            InitializeCore(context);

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
            SetParameter(ParticleBaseKeys.ColorIntensity, ColorIntensity);
            SetParameter(ParticleBaseKeys.ColorScaleMin, color);
            SetParameter(ParticleBaseKeys.ColorScaleMax, color);

            ///////////////
            // This should be CB0 - view/proj matrices don't change per material
            SetParameter(ParticleBaseKeys.MatrixTransform, viewMatrix * projMatrix);

        }

        public virtual unsafe void PatchVertexBuffer(ParticleVertexLayout vtxBuilder, Vector3 invViewX, Vector3 invViewY, int maxVertices, ParticlePool pool, ParticleEmitter emitter = null)
        {
            var lifeField = pool.GetField(ParticleFields.RemainingLife);
            var randField = pool.GetField(ParticleFields.RandomSeed);

            if (!randField.IsValid() || !lifeField.IsValid())
                return;

            var colorField = pool.GetField(ParticleFields.Color);
            var hasColorField = colorField.IsValid();

            var whiteColor = new Color4(1, 1, 1, 1);

            // TODO Fetch sorted particles
            foreach (var particle in pool)
            {
                vtxBuilder.SetColorForParticle(hasColorField ? particle[colorField] : (IntPtr)(&whiteColor));

                vtxBuilder.SetLifetimeForParticle(particle[lifeField]);

                vtxBuilder.SetRandomSeedForParticle(particle[randField]);

                vtxBuilder.NextParticle();
            }

            vtxBuilder.RestartBuffer();
        }

        ParameterKey<ShaderMixinSource> key = new ParameterKey<ShaderMixinSource>("MyKey"); 

        private void InitializeCore(RenderContext context)
        {
            if (isInitialized)
                return;
            isInitialized = true;

            if (EffectName == null) throw new ArgumentNullException("No EffectName specified");

            parameters.Add(key, new ShaderMixinSource());
            parameterCollections = new List<ParameterCollection> { parameters };

            // Setup the effect compiler
            effectInstance = new DefaultEffectInstance(parameterCollections);
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

            // Get or create parameter collection
            if (parameterCollectionGroup == null || parameterCollectionGroup.Effect != effect)
            {
                // It is quite inefficient if user is often switching effect without providing a matching ParameterCollectionGroup
                parameterCollectionGroup = new EffectParameterCollectionGroup(graphicsDevice, effect, parameterCollections);
            }
        }

    }
}
