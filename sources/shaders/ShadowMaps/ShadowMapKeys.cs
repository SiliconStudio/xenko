// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.ShadowMaps
{

    /// <summary>
    /// Keys used for shadow mapping.
    /// </summary>
    public static partial class ShadowMapKeys
    {
        /// <summary>
        /// Depth sampling texture.
        /// </summary>
        public static readonly ParameterKey<Texture> DepthTexture = ParameterKeys.New<Texture>();

        /// <summary>
        /// Final shadow map texture.
        /// </summary>
        public static readonly ParameterKey<Texture> Texture = ParameterKeys.New<Texture>();

        /// <summary>
        /// Maximum distance used by a shadow map.
        /// </summary>
        public static readonly ParameterKey<float> DistanceMax = ParameterKeys.New(0.0f);

        /// <summary>
        /// Light Offset
        /// </summary>
        public static readonly ParameterKey<Vector3> LightOffset = ParameterKeys.New(Vector3.Zero);
    }
}