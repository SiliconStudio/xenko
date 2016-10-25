// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.IO;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Assets.Serializers
{
    /// <summary>
    /// Default serializer used for all Yaml content
    /// </summary>
    internal class AssetYamlSerializer : IAssetSerializer, IAssetSerializerFactory
    {
        public object Load(Stream stream, string filePath, ILogger log, out bool aliasOccurred, out Dictionary<ObjectPath, OverrideType> overrides)
        {
            PropertyContainer properties;
            var result = YamlSerializer.Deserialize(stream, null, log != null ? new SerializerContextSettings { Logger = log } : null, out aliasOccurred, out properties);
            properties.TryGetValue(CustomObjectSerializerBackend.OverrideDictionaryKey, out overrides);
            return result;
        }

        public void Save(Stream stream, object asset, ILogger log = null)
        {
            YamlSerializer.Serialize(stream, asset, null, log != null ? new SerializerContextSettings { Logger = log } : null);
        }

        public IAssetSerializer TryCreate(string assetFileExtension)
        {
            return this;
        }
    }
}
