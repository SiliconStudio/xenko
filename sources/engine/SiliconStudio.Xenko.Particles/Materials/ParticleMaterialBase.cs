// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
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

        [DataMember(40)]
        [Display("Face culling")]
        public ParticleMaterialCulling FaceCulling;

        /// <summary>
        /// Parameters should be divided into several groups later.
        /// CB0 - Parameters like camera position, viewProjMatrix, Screen size, FOV, etc. which persist for all materials/emitters in the same stage
        /// CB1 - Material attributes which persist for all batched together emitters
        /// CB2 - (Maybe) Per-emitter attributes.
        /// </summary>
        [DataMemberIgnore]
        protected readonly ParameterCollection Parameters = new ParameterCollection();

        /// <summary>
        /// Setups the current material using the graphics device.
        /// </summary>
        /// <param name="GraphicsDevice">Graphics device to setup</param>
        /// <param name="viewMatrix">The camera's View matrix</param>
        /// <param name="projMatrix">The camera's Projection matrix</param>
        public abstract void Setup(GraphicsDevice GraphicsDevice, Matrix viewMatrix, Matrix projMatrix);

        protected void SetupBase(GraphicsDevice graphicsDevice)
        {
            if (FaceCulling == ParticleMaterialCulling.CullNone)   graphicsDevice.SetRasterizerState(graphicsDevice.RasterizerStates.CullNone);
            if (FaceCulling == ParticleMaterialCulling.CullBack)   graphicsDevice.SetRasterizerState(graphicsDevice.RasterizerStates.CullBack);
            if (FaceCulling == ParticleMaterialCulling.CullFront)  graphicsDevice.SetRasterizerState(graphicsDevice.RasterizerStates.CullFront);

            graphicsDevice.SetBlendState(graphicsDevice.BlendStates.AlphaBlend);

            graphicsDevice.SetDepthStencilState(graphicsDevice.DepthStencilStates.DepthRead);

            // This is correct. We invert the value to reduce calculations on the shader side.
            Parameters.Set(ParticleBaseKeys.AlphaAdditive, 1f - AlphaAdditive);
        }
    }
}
