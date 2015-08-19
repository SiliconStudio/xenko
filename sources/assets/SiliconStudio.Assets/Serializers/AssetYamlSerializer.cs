// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.IO;

using SharpYaml.Serialization;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets.Serializers
{
    /// <summary>
    /// Default serializer used for all Yaml content
    /// </summary>
    internal class AssetYamlSerializer : IAssetSerializer, IAssetSerializerFactory
    {
        public object Load(Stream stream, string assetFileExtension, ILogger log, out bool aliasOccurred)
        {
            return YamlSerializer.Deserialize(stream, null, log != null ? new SerializerContextSettings() { Logger = new YamlForwardLogger(log) } : null, out aliasOccurred);
        }

        public void Save(Stream stream, object asset, ILogger log)
        {
            YamlSerializer.Serialize(stream, asset, null, log != null ? new SerializerContextSettings() { Logger = new YamlForwardLogger(log) } : null);
        }

        public IAssetSerializer TryCreate(string assetFileExtension)
        {
            return this;
        }
    }
}