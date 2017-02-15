// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using SiliconStudio.Assets.Yaml;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Assets.Serializers
{
    /// <summary>
    /// Default serializer used for all Yaml content
    /// </summary>
    public class YamlAssetSerializer : IAssetSerializer, IAssetSerializerFactory
    {
        public object Load(Stream stream, UFile filePath, ILogger log, out bool aliasOccurred, out AttachedYamlAssetMetadata yamlMetadata)
        {
            PropertyContainer properties;
            var result = AssetYamlSerializer.Default.Deserialize(stream, null, log != null ? new SerializerContextSettings { Logger = log } : null, out aliasOccurred, out properties);
            yamlMetadata = AttachedYamlAssetMetadata.FromPropertyContainer(properties);
            var objectReferences = yamlMetadata.RetrieveMetadata(AssetObjectSerializerBackend.ObjectReferencesKey);
            if (objectReferences != null)
            {
                FixupObjectReference.RunFixupPass(result, objectReferences, true, log);
            }
            return result;
        }

        public void Save(Stream stream, object asset, AttachedYamlAssetMetadata yamlMetadata, ILogger log = null)
        {
            var settings = new SerializerContextSettings(log);
            var overrides = yamlMetadata?.RetrieveMetadata(AssetObjectSerializerBackend.OverrideDictionaryKey);
            if (overrides != null)
            {
                settings.Properties.Add(AssetObjectSerializerBackend.OverrideDictionaryKey, overrides);
            }
            var objectReferences = yamlMetadata?.RetrieveMetadata(AssetObjectSerializerBackend.ObjectReferencesKey);
            if (objectReferences != null)
            {
                settings.Properties.Add(AssetObjectSerializerBackend.ObjectReferencesKey, objectReferences);
            }
            AssetYamlSerializer.Default.Serialize(stream, asset, null, settings);
        }

        public IAssetSerializer TryCreate(string assetFileExtension)
        {
            return this;
        }
    }
}
