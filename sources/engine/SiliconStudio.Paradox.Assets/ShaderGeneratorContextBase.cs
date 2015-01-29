// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using SiliconStudio.Assets;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Assets.Textures;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets
{
    /// <summary>
    /// Base class for generating shader class source with associated parameters.
    /// </summary>
    public abstract class ShaderGeneratorContextBase
    {
        // TODO: Document this class

        private readonly Dictionary<ParameterKey, int> parameterKeyIndices;

        private readonly Dictionary<SamplerStateDescription, ParameterKey<SamplerState>> declaredSamplerStates;

        public delegate Asset FindAssetDelegate(IContentReference reference);

        public FindAssetDelegate FindAsset { get; set; }

        public LoggerResult Log { get; set; }

        protected ShaderGeneratorContextBase()
        {
            Parameters = new ParameterCollection();
            parameterKeyIndices = new Dictionary<ParameterKey, int>();
            declaredSamplerStates = new Dictionary<SamplerStateDescription, ParameterKey<SamplerState>>();
        }

        protected ShaderGeneratorContextBase(Package package) : this()
        {
            FindAsset = reference =>
            {
                var assetItem = package.Session.FindAsset(reference.Id)
                                ?? package.Session.FindAsset(reference.Location);

                if (assetItem == null)
                {
                    return null;
                }
                return assetItem.Asset;
            };
        }

        public ParameterCollection Parameters { get; set; }

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

        public ParameterKey<Texture> GetTextureKey(IContentReference textureReference, ParameterKey<Texture> key)
        {
            var textureKey = (ParameterKey<Texture>)GetParameterKey(key);
            if (textureReference != null)
            {
                var texture = AttachedReferenceManager.CreateSerializableVersion<Texture>(textureReference.Id, textureReference.Location);
                Parameters.Set(textureKey, texture);
            }
            return textureKey;
        }

        public ParameterKey<SamplerState> GetSamplerKey(SamplerStateDescription samplerStateDesc)
        {
            ParameterKey<SamplerState> key;

            if (!declaredSamplerStates.TryGetValue(samplerStateDesc, out key))
            {
                key = MaterialKeys.Sampler.ComposeWith("i" + declaredSamplerStates.Count.ToString(CultureInfo.InvariantCulture));
                declaredSamplerStates.Add(samplerStateDesc, key);
            }

            var samplerState = new SamplerState(samplerStateDesc);
            Parameters.Set(key, samplerState);
            return key;
        }
    }
}