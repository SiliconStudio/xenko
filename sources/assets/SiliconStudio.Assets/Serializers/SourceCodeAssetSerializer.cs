// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets.Serializers
{
    internal class SourceCodeAssetSerializer : IAssetSerializer, IAssetSerializerFactory
    {
        public static readonly SourceCodeAssetSerializer Default = new SourceCodeAssetSerializer();

        public object Load(Stream stream, UFile filePath, ILogger log, out bool aliasOccurred, out Dictionary<YamlAssetPath, OverrideType> overrides)
        {
            aliasOccurred = false;

            var assetFileExtension = filePath.GetFileExtension().ToLowerInvariant();
            var type = AssetRegistry.GetAssetTypeFromFileExtension(assetFileExtension);
            var asset = (SourceCodeAsset)Activator.CreateInstance(type);

            var textAccessor = asset.TextAccessor as SourceCodeAsset.DefaultTextAccessor;
            if (textAccessor != null)
            {
                // Don't load the file if we have the file path
                textAccessor.FilePath = filePath;

                // Set the assets text if it loaded from an in-memory version
                // TODO: Propagate dirtiness?
                if (stream is MemoryStream)
                {
                    var reader = new StreamReader(stream, Encoding.UTF8);
                    textAccessor.Set(reader.ReadToEnd());
                }
            }

            // No override in source code assets
            overrides = new Dictionary<YamlAssetPath, OverrideType>();

            return asset;
        }

        public void Save(Stream stream, object asset, ILogger log = null, Dictionary<YamlAssetPath, OverrideType> overrides = null)
        {
            ((SourceCodeAsset)asset).Save(stream);
        }

        public IAssetSerializer TryCreate(string assetFileExtension)
        {
            var assetType = AssetRegistry.GetAssetTypeFromFileExtension(assetFileExtension);
            if (assetType != null && typeof(SourceCodeAsset).IsAssignableFrom(assetType))
            {
                return this;
            }
            return null;
        }
    }
}
