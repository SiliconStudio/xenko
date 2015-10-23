// Copyright (c) 2011 Silicon Studio

using System;
using System.Runtime.InteropServices;

using Xenko.Framework.Graphics;
using Xenko.Framework.Mathematics;

namespace Xenko.Effects.Modules
{

    /// <summary>
    /// Keys used for shadow mapping.
    /// </summary>
    public static class ShadowMapKeys
    {
        /// <summary>
        /// Depth sampling texture.
        /// </summary>
        public static readonly ParameterResourceKey<Texture2D> DepthTexture = ParameterKeys.Resource<Texture2D>();

        /// <summary>
        /// Final shadow map texture.
        /// </summary>
        public static readonly ParameterResourceKey<Texture2D> Texture = ParameterKeys.Resource<Texture2D>();

        /// <summary>
        /// TODO comment this sampler.
        /// </summary>
        public static readonly ParameterResourceKey<SamplerState> Sampler = ParameterKeys.Resource<SamplerState>();

        /// <summary>
        /// TODO comment this sampler.
        /// </summary>
        public static readonly ParameterResourceKey<SamplerState> Sampler2 = ParameterKeys.Resource<SamplerState>();

        /// <summary>
        /// Maximum distance used by a shadow map.
        /// </summary>
        public static readonly ParameterValueKey<float> DistanceMax = ParameterKeys.Value(0.0f);
    }
}