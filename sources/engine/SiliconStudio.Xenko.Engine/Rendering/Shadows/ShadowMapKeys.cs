// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Rendering.Shadows
{

    /// <summary>
    /// Keys used for shadow mapping.
    /// </summary>
    public static partial class ShadowMapKeys
    {
        /// <summary>
        /// Final shadow map texture.
        /// </summary>
        public static readonly ParameterKey<Texture> Texture = ParameterKeys.New<Texture>();

        /// <summary>
        /// Final shadow map texture size
        /// </summary>
        public static readonly ParameterKey<Vector2> TextureSize = ParameterKeys.New<Vector2>();

        /// <summary>
        /// Final shadow map texture texel size.
        /// </summary>
        public static readonly ParameterKey<Vector2> TextureTexelSize = ParameterKeys.New<Vector2>();
    }
}