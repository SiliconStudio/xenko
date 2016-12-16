// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Xenko.Assets.Models
{
    partial class AnimationAsset
    {
        /// <summary>
        /// Converts AdditiveAnimationAsset to AnimationAsset.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Upgrader from version 1.5.0-alpha02 to 1.9.3-beta01 (AnimationAsset).</item>
        /// </list>
        /// </remarks>
        protected sealed class AnimationAssetRemoveAdditiveUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile,
                OverrideUpgraderHint overrideHint)
            {
                var assetTag = asset.Node.Tag;
                if (assetTag != "!AdditiveAnimation")
                    return;

                asset.Node.Tag = "!Animation";
                dynamic newType = new DynamicYamlMapping(new YamlMappingNode());
                newType.Node.Tag = "!DifferenceAnimationAssetType";
                newType["BaseSource"] = asset["BaseSource"];
                newType["Mode"] = asset["Mode"];

                asset.RemoveChild("BaseSource");
                asset.RemoveChild("Mode");
                asset.RemoveChild("Type");

                asset.AddChild("Type", newType);

            }
        }
    }
}
