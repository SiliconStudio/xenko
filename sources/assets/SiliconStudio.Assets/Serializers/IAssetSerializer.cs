// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets.Serializers
{
    public interface IAssetSerializerFactory
    {
        IAssetSerializer TryCreate(string assetFileExtension);
    }

    public interface IAssetSerializer
    {
        object Load(Stream stream, UFile filePath, ILogger log, out bool aliasOccurred, out Dictionary<YamlAssetPath, OverrideType> overrides);

        void Save(Stream stream, object asset, ILogger log = null, Dictionary<YamlAssetPath, OverrideType> overrides = null, Dictionary<YamlAssetPath, Guid> objectReferences = null);
    }
}
