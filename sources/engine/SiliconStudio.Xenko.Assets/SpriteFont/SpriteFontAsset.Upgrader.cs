// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SharpYaml.Serialization;
using SiliconStudio.Assets;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    public partial class SpriteFontAsset
    {
        /// <summary>
        /// Removes the IsDynamic checkbox and changes it with an enum (Static, Dynamic, SDF)
        /// </summary>
        class FontTypeUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile,
                OverrideUpgraderHint overrideHint)
            {
                if (asset.IsDynamic != null)
                {
                    var isDynamic = (bool)asset.IsDynamic;

                    // There is also SDF type, but old assets don't have it yet
                    asset.AddChild("FontType", isDynamic ? "Dynamic" : "Static");

                    asset.RemoveChild("IsDynamic");
                }
            }

        }

        /// <summary>
        /// Removes the enum (Static, Dynamic, SDF) and changes them with an abstract sub-module while also moving the character regions to the sub-module
        /// </summary>
        class FontClassUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile,
                OverrideUpgraderHint overrideHint)
            {
                dynamic newSource = new DynamicYamlMapping(new YamlMappingNode());

                var assetName = (asset.FontName != null) ? (string)asset.FontName : null;
                var assetSource = (asset.Source != null) ? (string)asset.Source : null;

                // First check if the asset has a valid source
                if (assetSource != null && !assetSource.IsNullOrEmpty() && !assetSource.Equals("null"))
                {
                    newSource.Node.Tag = "!FileFontProvider";
                    newSource.AddChild("Source", assetSource);
                }

                // Only if the asset doesn't have a valid source can it be a system font
                else
                if (assetName != null && !assetName.IsNullOrEmpty() && !assetName.Equals("null"))
                {
                    newSource.Node.Tag = "!SystemFontProvider";
                    newSource.AddChild("FontName", assetName);

                    if (asset.Style != null)
                    {
                        newSource.AddChild("Style", asset.Style);
                    }
                }



                asset.RemoveChild("FontName");
                asset.RemoveChild("Source");
                asset.RemoveChild("Style");

                asset.AddChild("FontSource", newSource);


                if (asset.FontType != null)
                {
                    var fontType = (string)asset.FontType;
                    asset.RemoveChild("FontType");

                    dynamic newType = new DynamicYamlMapping(new YamlMappingNode());

                    if (fontType.Equals("Dynamic"))
                    {
                        newType.Node.Tag = "!RuntimeRasterizedSpriteFontType";

                        if (asset.Size != null)
                            newType.AddChild("Size", asset.Size);

                        if (asset.AntiAlias != null)
                            newType.AddChild("AntiAlias", asset.AntiAlias);
                    }
                    else
                    if (fontType.Equals("SDF"))
                    {
                        newType.Node.Tag = "!SignedDistanceFieldSpriteFontType";

                        if (asset.Size != null)
                            newType.AddChild("Size", asset.Size);

                        if (asset.CharacterSet != null)
                            newType.AddChild("CharacterSet", asset.CharacterSet);

                        if (asset.CharacterRegions != null)
                            newType.AddChild("CharacterRegions", asset.CharacterRegions);
                    }
                    else
                    {
                        newType.Node.Tag = "!OfflineRasterizedSpriteFontType";

                        if (asset.Size != null)
                            newType.AddChild("Size", asset.Size);

                        if (asset.CharacterSet != null)
                            newType.AddChild("CharacterSet", asset.CharacterSet);

                        if (asset.CharacterRegions != null)
                            newType.AddChild("CharacterRegions", asset.CharacterRegions);

                        if (asset.AntiAlias != null)
                            newType.AddChild("AntiAlias", asset.AntiAlias);

                        if (asset.IsPremultiplied != null)
                            newType.AddChild("IsPremultiplied", asset.IsPremultiplied);
                    }

                    asset.AddChild("FontType", newType);
                }

                asset.RemoveChild("IsPremultiplied");
                asset.RemoveChild("AntiAlias");
                asset.RemoveChild("UseKerning");
                asset.RemoveChild("Size");
                asset.RemoveChild("CharacterSet");
                asset.RemoveChild("CharacterRegions");
            }
        }



        /// <summary>
        /// Upgrades the font size from points to pixels
        /// </summary>
        class FontSizeUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile,
                OverrideUpgraderHint overrideHint)
            {
                if (asset.FontType == null)
                    return;

                if (asset.FontType.Size == null)
                {
                    //  It is possible our font type has the default size of 16, which translates to 21.33333 in pixel size
                    asset.FontType.AddChild("Size", 21.33333);
                    return;
                }

                var newSize = ((float)asset.FontType.Size) * 1.3333333f;

                asset.FontType.RemoveChild("Size");

                asset.FontType.AddChild("Size", newSize);
            }
        }
    }
}
