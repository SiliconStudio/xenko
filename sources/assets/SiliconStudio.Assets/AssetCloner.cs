// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Allows to clone an asset or values stored in an asset.
    /// </summary>
    public struct AssetCloner
    {
        private readonly object streamOrValueType;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetCloner" /> struct.
        /// </summary>
        /// <param name="value">The value to clone.</param>
        /// <param name="keepOnlySealedOverride">if set to <c>true</c> to discard override information except sealed.</param>
        public AssetCloner(object value, bool keepOnlySealedOverride = false)
        {
            // Clone only if value is not a value type
            if (value != null && !value.GetType().IsValueType)
            {
                // TODO Clone is not supporting SourceCodeAsset (The SourceCodeAsset.Text won't be cloned)
                var stream = new MemoryStream();
                YamlSerializer.Serialize(stream, value, keepOnlySealedOverride);
                streamOrValueType = stream;
            }
            else
            {
                streamOrValueType = value;
            }
        }

        /// <summary>
        /// Clones the current value of this cloner with the specified new shadow registry (optional)
        /// </summary>
        /// <returns>A clone of the value associated with this cloner.</returns>
        public object Clone()
        {
            var stream = streamOrValueType as Stream;
            if (stream != null)
            {
                stream.Position = 0;
                var newObject = YamlSerializer.Deserialize(stream);
                return newObject;
            }
            // Else this is a value type, so it is cloned automatically
            return streamOrValueType;
        }

        /// <summary>
        /// Clones the specified asset using asset serialization.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="keepOnlySealedOverride">if set to <c>true</c> to discard override information except sealed.</param>
        /// <returns>A clone of the asset.</returns>
        /// TODO: This code is not efficient as it is using YAML serialization for cloning assets
        public static object Clone(object asset, bool keepOnlySealedOverride = false)
        {
            if (asset == null)
            {
                return null;
            }
            var cloner = new AssetCloner(asset, keepOnlySealedOverride);
            return cloner.Clone();
        }
    }
}