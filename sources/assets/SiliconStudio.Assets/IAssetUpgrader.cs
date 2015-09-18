// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SharpYaml.Serialization;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets
{
    public interface IAssetUpgrader
    {
        void Upgrade(AssetMigrationContext context, int currentVersion, int targetVersion, YamlMappingNode yamlAssetNode, PackageLoadingAssetFile assetFile);
    }
}