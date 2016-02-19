// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Globalization;

using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;

namespace SiliconStudio.Xenko.Assets
{
    /// <summary>
    /// Base class for generating shader class source with associated parameters.
    /// </summary>
    public abstract class ShaderGeneratorContextBase : ComponentBase
    {
        // TODO: Document this class

        private readonly Dictionary<ParameterKey, int> parameterKeyIndices;

        private readonly Dictionary<SamplerStateDescription, ObjectParameterKey<SamplerState>> declaredSamplerStates;

        private readonly Dictionary<Color4, Texture> singleColorTextures = new Dictionary<Color4, Texture>();

        public delegate object FindAssetDelegate(object asset);

        public delegate string GetAssetFriendlyNameDelegate(object asset);
        
        public FindAssetDelegate FindAsset { get; set; }

        public GetAssetFriendlyNameDelegate GetAssetFriendlyName { get; set; }

        public LoggerResult Log { get; set; }

        /// <summary>
        /// Gets or sets the asset manager.
        /// </summary>
        /// <value>
        /// The asset manager.
        /// </value>
        public AssetManager Assets { get; set; }

        protected ShaderGeneratorContextBase()
        {
            Parameters = new NextGenParameterCollection();
            parameterKeyIndices = new Dictionary<ParameterKey, int>();
            declaredSamplerStates = new Dictionary<SamplerStateDescription, ObjectParameterKey<SamplerState>>();
        }

        public NextGenParameterCollection Parameters { get; set; }

        public ParameterKey GetParameterKey(ParameterKey key)
        {
            if (key == null) throw new ArgumentNullException("key");

            var baseKey = key;
            int parameterKeyIndex;
            parameterKeyIndices.TryGetValue(baseKey, out parameterKeyIndex);

            key = parameterKeyIndex == 0 ? baseKey : baseKey.ComposeWith("i"+parameterKeyIndex.ToString(CultureInfo.InvariantCulture));

            parameterKeyIndex++;
            parameterKeyIndices[baseKey] = parameterKeyIndex;
            return key;
        }

        public unsafe Texture GenerateTextureFromColor(Color color)
        {
            if (Assets == null)
            {
                Log.Error("Trying to generate a texture without an AssetManager");
                return null;
            }

            // Already generated?
            Texture texture;
            if (singleColorTextures.TryGetValue(color, out texture))
                return texture;

            // Generate 1x1 texture of given color
            var image = Image.New2D(1, 1, 1, PixelFormat.R8G8B8A8_UNorm);
            *((Color*)image.PixelBuffer[0].DataPointer) = color;

            texture = image.ToSerializableVersion();

            // Save texture
            Assets.Save(string.Format("__material_internal__/color_texture_{0:X2}{1:X2}{2:X2}{3:X2}", color.R, color.G, color.B, color.A), texture);

            singleColorTextures.Add(color, texture);

            return texture;
        }

        public ObjectParameterKey<Texture> GetTextureKey(Texture texture, ObjectParameterKey<Texture> key, Color? defaultTextureValue = null)
        {
            var textureKey = (ObjectParameterKey<Texture>)GetParameterKey(key);
            if (texture != null)
            {
                Parameters.Set(textureKey, texture);
            }
            else if (defaultTextureValue != null && Assets != null)
            {
                texture = GenerateTextureFromColor(defaultTextureValue.Value);
                Parameters.Set(textureKey, texture);
            }
            return textureKey;
        }

        public ObjectParameterKey<SamplerState> GetSamplerKey(SamplerStateDescription samplerStateDesc)
        {
            ObjectParameterKey<SamplerState> key;

            if (!declaredSamplerStates.TryGetValue(samplerStateDesc, out key))
            {
                key = MaterialKeys.Sampler.ComposeWith("i" + declaredSamplerStates.Count.ToString(CultureInfo.InvariantCulture));
                declaredSamplerStates.Add(samplerStateDesc, key);
            }

            var samplerState = SamplerState.NewFake(samplerStateDesc);
            Parameters.Set(key, samplerState);
            return key;
        }
    }
}