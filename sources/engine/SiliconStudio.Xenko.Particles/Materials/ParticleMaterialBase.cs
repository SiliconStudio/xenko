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
        public float ColorIntensity { get; set; } = 1f;

        [DataMember(40)]
        [Display("Face culling")]
        public ParticleMaterialCulling FaceCulling;

        [DataMemberIgnore]
        public ParticleEffectVariation MandatoryVariation { get; protected set; } = ParticleEffectVariation.None;

        /// <summary>
        /// Parameters should be divided into several groups later.
        /// CB0 - Parameters like camera position, viewProjMatrix, Screen size, FOV, etc. which persist for all materials/emitters in the same stage
        /// CB1 - Material attributes which persist for all batched together emitters
        /// CB2 - (Maybe) Per-emitter attributes.
        /// </summary>
        [DataMemberIgnore]
        private readonly ParameterCollection Parameters = new ParameterCollection();

        protected EffectParameterCollectionGroup ParameterCollectionGroup { get; private set; } // Will move to effect instance

        /// <summary>
        /// Setups the current material using the graphics device.
        /// </summary>
        /// <param name="GraphicsDevice">Graphics device to setup</param>
        /// <param name="viewMatrix">The camera's View matrix</param>
        /// <param name="projMatrix">The camera's Projection matrix</param>
        public abstract void Setup(GraphicsDevice GraphicsDevice, RenderContext context, Matrix viewMatrix, Matrix projMatrix, Color4 color);

        public abstract void PatchVertexBuffer(ParticleVertexLayout vtxBuilder, Vector3 invViewX, Vector3 invViewY, int remainingCapacity, ParticlePool pool, ParticleEmitter emitter = null);

        [DataMemberIgnore]
        private Effect effect = null;

        private List<ParameterCollection> parameterCollections;

        private const string EffectName = "ParticleBatch";

        private bool isInitialized = false;

        private void InitializeCore(RenderContext context)
        {
            if (isInitialized)
                return;
            isInitialized = true;

            if (EffectName == null) throw new ArgumentNullException("No EffectName specified");

            parameterCollections = new List<ParameterCollection> { Parameters };

            // Setup the effect compiler
            EffectInstance = new DefaultEffectInstance(parameterCollections);
            effectCompiler = new DynamicEffectCompiler(context.Services, EffectName, -1); // Image effects are compiled with higher priority

        }

        protected void PrepareEffect(GraphicsDevice graphicsDevice, RenderContext context)
        {
            InitializeCore(context);

            if (FaceCulling == ParticleMaterialCulling.CullNone)   graphicsDevice.SetRasterizerState(graphicsDevice.RasterizerStates.CullNone);
            if (FaceCulling == ParticleMaterialCulling.CullBack)   graphicsDevice.SetRasterizerState(graphicsDevice.RasterizerStates.CullBack);
            if (FaceCulling == ParticleMaterialCulling.CullFront)  graphicsDevice.SetRasterizerState(graphicsDevice.RasterizerStates.CullFront);

            graphicsDevice.SetBlendState(graphicsDevice.BlendStates.AlphaBlend);

            graphicsDevice.SetDepthStencilState(graphicsDevice.DepthStencilStates.DepthRead);
                            
            // TODO Maybe replicate ResourceContext and have all vtx, idx buffers and binding to be on the material or shapebuilder side
            // graphicsDevice.SetVertexArrayObject(ResourceContext.VertexArrayObject);

            // This is correct. We invert the value to reduce calculations on the shader side.
            Parameters.Set(ParticleBaseKeys.AlphaAdditive, 1f - AlphaAdditive);

            // Scale up the color intensity - might depend on the eye adaptation later
            Parameters.Set(ParticleBaseKeys.ColorIntensity, ColorIntensity);
        }

        public void SetParameter<T>(ParameterKey<T> key, T value) => Parameters.Set(key, value);


        protected void ApplyEffect(GraphicsDevice graphicsDevice)
        {
            UpdateEffect(graphicsDevice);

            effect.Apply(graphicsDevice, ParameterCollectionGroup, applyEffectStates: false);
        }

        #region Dynamic effect
        protected DefaultEffectInstance EffectInstance;

        private DynamicEffectCompiler effectCompiler;

        private void UpdateEffect(GraphicsDevice graphicsDevice)
        {
            effectCompiler.Update(EffectInstance, null);

            effect = EffectInstance.Effect;

            // Get or create parameter collection
            if (ParameterCollectionGroup == null || ParameterCollectionGroup.Effect != effect)
            {
                // It is quite inefficient if user is often switching effect without providing a matching ParameterCollectionGroup
                ParameterCollectionGroup = new EffectParameterCollectionGroup(graphicsDevice, effect, parameterCollections);
            }
        }

        #endregion

    }
}
