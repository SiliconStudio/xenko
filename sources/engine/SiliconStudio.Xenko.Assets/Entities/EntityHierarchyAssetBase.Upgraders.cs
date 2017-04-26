// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Xenko.Assets.Entities
{
    partial class EntityHierarchyAssetBase
    {
        /// <summary>
        /// Moves Group from Entity to inside components (for those that support it)
        /// </summary>
        protected class MoveRenderGroupInsideComponentUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Parts;
                foreach (dynamic entityDesign in entities)
                {
                    var entity = entityDesign.Entity;

                    // Check if entity has a group (otherwise nothing to do
                    var group = entity.Group;
                    if (group == null)
                        continue;

                    // Save override and remove old element
                    var groupOverride = entity.GetOverride("Group");
                    entity.RemoveChild("Group");

                    foreach (var component in entity.Components)
                    {
                        try
                        {
                            var componentTag = component.Value.Node.Tag;
                            if (componentTag == "!ModelComponent"
                                || componentTag == "!SpriteComponent" || componentTag == "!UIComponent"
                                || componentTag == "!BackgroundComponent" || componentTag == "!SkyboxComponent"
                                || componentTag == "!ParticleSystemComponent"
                                || componentTag == "!SpriteStudioComponent")
                            {
                                component.Value.RenderGroup = group;
                                component.Value.SetOverride("RenderGroup", groupOverride);
                            }
                        }
                        catch (Exception e)
                        {
                            e.Ignore();
                        }
                    }
                }
            }
        }
    }
}
