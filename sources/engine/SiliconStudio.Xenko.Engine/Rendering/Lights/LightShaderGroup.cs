// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// A group of lights of the same type (single loop in the shader).
    /// </summary>
    public abstract class LightShaderGroup
    {
        protected LightShaderGroup()
        {
        }

        protected LightShaderGroup(ShaderSource mixin)
        {
            ShaderSource = mixin;
        }

        public ShaderSource ShaderSource { get; protected set; }

        public bool HasEffectPermutations { get; protected set; } = false;

        /// <summary>
        /// Called when layout is updated, so that parameter keys can be recomputed.
        /// </summary>
        /// <param name="compositionName"></param>
        public virtual void UpdateLayout(string compositionName)
        {
        }

        /// <summary>
        /// Resets states.
        /// </summary>
        public virtual void Reset()
        {
        }

        /// <summary>
        /// Applies effect permutations.
        /// </summary>
        /// <param name="renderEffect"></param>
        public virtual void ApplyEffectPermutations(RenderEffect renderEffect)
        {
        }

        /// <summary>
        /// Applies PerView lighting parameters.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="viewIndex"></param>
        /// <param name="parameters"></param>
        public virtual void ApplyViewParameters(RenderDrawContext context, int viewIndex, ParameterCollection parameters)
        {
        }

        /// <summary>
        /// Applies PerDraw lighting parameters.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="viewIndex"></param>
        /// <param name="parameters"></param>
        /// <param name="boundingBox"></param>
        public virtual void ApplyDrawParameters(RenderDrawContext context, int viewIndex, ParameterCollection parameters, ref BoundingBoxExt boundingBox)
        {
        }
    }

    public struct LightDynamicEntry
    {
        public readonly LightComponent Light;
        public readonly LightShadowMapTexture ShadowMapTexture;

        public LightDynamicEntry(LightComponent light, LightShadowMapTexture shadowMapTexture)
        {
            Light = light;
            ShadowMapTexture = shadowMapTexture;
        }
    }
}