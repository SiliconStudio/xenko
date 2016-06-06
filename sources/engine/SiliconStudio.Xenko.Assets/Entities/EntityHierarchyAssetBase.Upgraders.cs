// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Xenko.Assets.Entities
{
    partial class EntityHierarchyAssetBase
    {
        // All upgraders for EntityHierarchyAssetBase assets
        // Note: access level must be at least 'protected' so that derived asset classes (e.g. PrefabAsset, SceneAsset) can use them.

        /// <summary>
        /// CurrentFrame is now serialized in <see cref="Engine.ISpriteProvider"/> (was previously serialized at the <see cref="Engine.SpriteComponent"/> level.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Upgrader from version 0.0.0 to 1.7.0-beta01 (PrefabAsset).</item>
        /// <item>Upgrader from version 1.6.0-beta03 to 1.7.0-beta01 (SceneAsset).</item>
        /// </list>
        /// </remarks>
        protected sealed class SpriteComponentUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                var hierarchy = asset.Hierarchy;
                var entities = hierarchy.Entities;
                foreach (var entityAndDesign in entities)
                {
                    var entity = entityAndDesign.Entity;
                    foreach (var component in entity.Components)
                    {
                        var componentTag = component.Node.Tag;
                        if (componentTag != "!SpriteComponent")
                            continue;

                        var provider = component.SpriteProvider;
                        if (provider == null || provider.Node.Tag != "!SpriteFromSheet")
                            continue;

                        provider.AddChild("CurrentFrame", component.CurrentFrame);
                        component.RemoveChild("CurrentFrame");
                    }
                }
            }
        }

        /// <summary>
        /// UIComponent now has Resolution and ResolutionStretch properties.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Upgrader from version 1.7.0-beta01 to 1.7.0-beta02 (PrefabAsset).</item>
        /// <item>Upgrader from version 1.7.0-beta01 to 1.7.0-beta02 (SceneAsset).</item>
        /// </list>
        /// </remarks>
        protected sealed class UIComponentRenamingResolutionUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                var hierarchy = asset.Hierarchy;
                var entities = hierarchy.Entities;
                foreach (var entityAndDesign in entities)
                {
                    var entity = entityAndDesign.Entity;
                    foreach (var component in entity.Components)
                    {
                        var componentTag = component.Node.Tag;
                        if (componentTag != "!UIComponent")
                            continue;

                        // VirtualResolution
                        var virtualResolution = component.VirtualResolution;
                        var vrAsMap = virtualResolution as DynamicYamlMapping;
                        if (vrAsMap != null)
                        {
                            component.AddChild("Resolution", virtualResolution);
                            component.RemoveChild("VirtualResolution");
                        }

                        // VirtualResolutionMode
                        var resolutionStretch = component.VirtualResolutionMode;
                        var vrmAsMap = resolutionStretch as DynamicYamlScalar;
                        if (vrmAsMap != null)
                        {
                            component.AddChild("ResolutionStretch", resolutionStretch);
                            component.RemoveChild("VirtualResolutionMode");
                        }
                    }
                }
            }
        }
    }
}
