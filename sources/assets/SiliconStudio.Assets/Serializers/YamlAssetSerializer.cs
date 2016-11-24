// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.IO;
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
        public object Load(Stream stream, UFile filePath, ILogger log, out bool aliasOccurred, out Dictionary<YamlAssetPath, OverrideType> overrides)
        {
            PropertyContainer properties;
            var result = AssetYamlSerializer.Default.Deserialize(stream, null, log != null ? new SerializerContextSettings { Logger = log } : null, out aliasOccurred, out properties);
            properties.TryGetValue(AssetObjectSerializerBackend.OverrideDictionaryKey, out overrides);
            return result;
        }

        public void Save(Stream stream, object asset, ILogger log = null, Dictionary<YamlAssetPath, OverrideType> overrides = null)
        {
            var settings = new SerializerContextSettings(log);
            if (overrides != null)
            {
                settings.Properties.Add(AssetObjectSerializerBackend.OverrideDictionaryKey, overrides);
            }
            AssetYamlSerializer.Default.Serialize(stream, asset, null, settings);
        }

        public IAssetSerializer TryCreate(string assetFileExtension)
        {
            return this;
        }
    }
}
