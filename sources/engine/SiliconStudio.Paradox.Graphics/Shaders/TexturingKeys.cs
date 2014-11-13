// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Modules
{
    public partial class TexturingKeys
    {
        static TexturingKeys()
        {
            Texture0TexelSize = CreateDynamicTexelSizeParameterKey(Texture0);
            Texture1TexelSize = CreateDynamicTexelSizeParameterKey(Texture1);
            Texture2TexelSize = CreateDynamicTexelSizeParameterKey(Texture2);
            Texture3TexelSize = CreateDynamicTexelSizeParameterKey(Texture3);
            Texture4TexelSize = CreateDynamicTexelSizeParameterKey(Texture4);
            Texture5TexelSize = CreateDynamicTexelSizeParameterKey(Texture5);
            Texture6TexelSize = CreateDynamicTexelSizeParameterKey(Texture6);
            Texture7TexelSize = CreateDynamicTexelSizeParameterKey(Texture7);
            Texture8TexelSize = CreateDynamicTexelSizeParameterKey(Texture8);
            Texture9TexelSize = CreateDynamicTexelSizeParameterKey(Texture9);
        }

        /// <summary>
        /// Creates a dynamic parameter key for texel size updated from the texture size.
        /// </summary>
        /// <param name="textureKey">Key of the texture to take the size from</param>
        /// <returns>A dynamic TexelSize parameter key updated according to the specified texture</returns>
        public static ParameterKey<Vector2> CreateDynamicTexelSizeParameterKey(ParameterKey<Texture> textureKey)
        {
            if (textureKey == null) throw new ArgumentNullException("textureKey");
            return ParameterKeys.NewDynamic(ParameterDynamicValue.New<Vector2, Texture>(textureKey, UpdateTexelSize));
        }

        /// <summary>
        /// Updates the size of the texel from a texture.
        /// </summary>
        /// <param name="param1">The param1.</param>
        /// <param name="output">The output.</param>
        private static void UpdateTexelSize(ref Texture param1, ref Vector2 output)
        {
            output = (param1 != null) ? new Vector2(param1.Width, param1.Height) : Vector2.Zero;
        }
    }
}