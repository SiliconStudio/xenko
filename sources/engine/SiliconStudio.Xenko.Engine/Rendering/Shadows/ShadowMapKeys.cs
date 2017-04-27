// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Shadows
{

    /// <summary>
    /// Keys used for shadow mapping.
    /// </summary>
    public static partial class ShadowMapKeys
    {
        /// <summary>
        /// Final shadow map texture.
        /// </summary>
        public static readonly ObjectParameterKey<Texture> Texture = ParameterKeys.NewObject<Texture>();

        /// <summary>
        /// Final shadow map texture size
        /// </summary>
        public static readonly ValueParameterKey<Vector2> TextureSize = ParameterKeys.NewValue<Vector2>();

        /// <summary>
        /// Final shadow map texture texel size.
        /// </summary>
        public static readonly ValueParameterKey<Vector2> TextureTexelSize = ParameterKeys.NewValue<Vector2>();
    }
}
