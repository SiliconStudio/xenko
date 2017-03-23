// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Serialization;

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
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile,
                OverrideUpgraderHint overrideHint)
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

                        component.TransferChild("CurrentFrame", provider, "CurrentFrame");
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
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile,
                OverrideUpgraderHint overrideHint)
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
                        component.RenameChild("VirtualResolution", "Resolution");

                        // VirtualResolutionMode
                        component.RenameChild("VirtualResolutionMode", "ResolutionStretch");
                    }
                }
            }
        }

        /// <summary>
        /// UpdaterColorOverTime now uses a ComputeCurveSamplerColor4 instead of a ComputeCurveSamplerVector4.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Upgrader from version 1.7.0-beta02 to 1.7.0-beta03 (PrefabAsset).</item>
        /// <item>Upgrader from version 1.7.0-beta02 to 1.7.0-beta03 (SceneAsset).</item>
        /// </list>
        /// </remarks>
        protected sealed class ParticleColorAnimationUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile,
                OverrideUpgraderHint overrideHint)
            {
                // Replace ComputeCurveSamplerVector4 with ComputeCurveSamplerColor4.
                // Replace ComputeAnimationCurveVector4 with ComputeAnimationCurveColor4.
                // Replace Vector4 with Color4.
                Action<dynamic> updateSampler = sampler =>
                {
                    if (sampler == null || sampler.Node.Tag != "!ComputeCurveSamplerVector4")
                        return;

                    sampler.Node.Tag = "!ComputeCurveSamplerColor4";

                    var curve = sampler.Curve;
                    curve.Node.Tag = "!ComputeAnimationCurveColor4";
                    foreach (var kf in curve.KeyFrames)
                    {
                        var colorValue = new DynamicYamlMapping(new YamlMappingNode());
                        colorValue.AddChild("R", kf.Value.X);
                        colorValue.AddChild("G", kf.Value.Y);
                        colorValue.AddChild("B", kf.Value.Z);
                        colorValue.AddChild("A", kf.Value.W);

                        kf.Value = colorValue;
                    }
                };

                var hierarchy = asset.Hierarchy;
                var entities = hierarchy.Entities;
                foreach (var entityAndDesign in entities)
                {
                    var entity = entityAndDesign.Entity;
                    foreach (var component in entity.Components)
                    {
                        var componentTag = component.Node.Tag;
                        if (componentTag != "!ParticleSystemComponent")
                            continue;

                        var particleSystem = component.ParticleSystem;
                        if (particleSystem == null)
                            continue;

                        foreach (var emitter in particleSystem.Emitters)
                        {
                            // Updaters
                            foreach (var updater in emitter.Updaters)
                            {
                                var updaterTag = updater.Node.Tag;
                                if (updaterTag != "!UpdaterColorOverTime")
                                    continue;

                                // Update the samplers
                                updateSampler(updater.SamplerMain);
                                updateSampler(updater.SamplerOptional);
                            }
                        }
                    }
                }
            }
        }

        /// <remarks>
        /// <list type="bullet">
        /// <item>Upgrader from version 1.7.0-beta03 to 1.7.0-beta04 (PrefabAsset).</item>
        /// <item>Upgrader from version 1.7.0-beta03 to 1.7.0-beta04 (SceneAsset).</item>
        /// </list>
        /// </remarks>
        protected sealed class EntityDesignUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile,
                OverrideUpgraderHint overrideHint)
            {
                asset.Hierarchy.RootPartIds = asset.Hierarchy.RootEntities;
                asset.Hierarchy.Parts = asset.Hierarchy.Entities;
                asset.Hierarchy.RootEntities = DynamicYamlEmpty.Default;
                asset.Hierarchy.Entities = DynamicYamlEmpty.Default;
                foreach (var entityDesign in asset.Hierarchy.Parts)
                {
                    entityDesign.Folder = entityDesign.Design.Folder;
                    entityDesign.BaseId = entityDesign.Design.BaseId;
                    entityDesign.BasePartInstanceId = entityDesign.Design.BasePartInstanceId;
                    entityDesign.Design = DynamicYamlEmpty.Default;
                }
            }
        }

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

        /// <remarks>
        /// <list type="bullet">
        /// <item>Upgrader from version 1.7.0-beta04 to 1.9.0-beta01 (PrefabAsset).</item>
        /// <item>Upgrader from version 1.7.0-beta04 to 1.9.0-beta01 (SceneAsset).</item>
        /// </list>
        /// </remarks>
        protected class CharacterSlopeUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile,
                OverrideUpgraderHint overrideHint)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Parts;
                foreach (dynamic entityDesign in entities)
                {
                    var entity = entityDesign.Entity;
                    foreach (var component in entity.Components)
                    {
                        var componentTag = component.Node.Tag;
                        if (componentTag == "!CharacterComponent")
                        {
                            var rads = component.MaxSlope;
                            var angle = new DynamicYamlMapping(new YamlMappingNode());
                            angle.AddChild("Radians", rads);
                            component.MaxSlope = angle;
                        }
                    }
                }
            }
        }

        /// <remarks>
        /// <list type="bullet">
        /// <item>Upgrader from version 1.9.0-beta01 to 1.9.0-beta02 (PrefabAsset).</item>
        /// <item>Upgrader from version 1.9.0-beta02 to 1.9.0-beta03 (SceneAsset).</item>
        /// </list>
        /// </remarks>
        protected class IdentifiableComponentUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile,
                OverrideUpgraderHint overrideHint)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Parts;
                foreach (dynamic entityDesign in entities)
                {
                    var entity = entityDesign.Entity;
                    foreach (var component in entity.Components)
                    {
                        component.Id = component["~Id"];
                        component["~Id"] = DynamicYamlEmpty.Default;
                    }
                }
            }
        }

        /// <remarks>
        /// <list type="bullet">
        /// <item>Upgrader from version 1.9.0-beta02 to 1.9.0-beta03 (PrefabAsset).</item>
        /// <item>Upgrader from version 1.9.0-beta03 to 1.9.0-beta04 (SceneAsset).</item>
        /// </list>
        /// </remarks>
        protected class BasePartsRemovalComponentUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile,
                OverrideUpgraderHint overrideHint)
            {
                var basePartMapping = new Dictionary<string, string>();
                if (asset["~BaseParts"] != null)
                {
                    foreach (dynamic basePart in asset["~BaseParts"])
                    {
                        try
                        {
                            var location = ((YamlScalarNode)basePart.Location.Node).Value;
                            var id = ((YamlScalarNode)basePart.Asset.Id.Node).Value;
                            var assetUrl = $"{id}:{location}";

                            foreach (dynamic part in basePart.Asset.Hierarchy.Parts)
                            {
                                try
                                {
                                    var partId = ((YamlScalarNode)part.Entity.Id.Node).Value;
                                    basePartMapping[partId] = assetUrl;
                                }
                                catch (Exception e)
                                {
                                    e.Ignore();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            e.Ignore();
                        }
                    }
                    asset["~BaseParts"] = DynamicYamlEmpty.Default;
                }
                var entities = (DynamicYamlArray)asset.Hierarchy.Parts;
                foreach (dynamic entityDesign in entities)
                {
                    if (entityDesign.BaseId != null)
                    {
                        try
                        {
                            var baseId = ((YamlScalarNode)entityDesign.BaseId.Node).Value;
                            var baseInstanceId = ((YamlScalarNode)entityDesign.BasePartInstanceId.Node).Value;
                            string assetUrl;
                            if (basePartMapping.TryGetValue(baseId, out assetUrl))
                            {
                                var baseNode = (dynamic)(new DynamicYamlMapping(new YamlMappingNode()));
                                baseNode.BasePartAsset = assetUrl;
                                baseNode.BasePartId = baseId;
                                baseNode.InstanceId = baseInstanceId;
                                entityDesign.Base = baseNode;
                            }
                            entityDesign.BaseId = DynamicYamlEmpty.Default;
                            entityDesign.BasePartInstanceId = DynamicYamlEmpty.Default;
                        }
                        catch (Exception e)
                        {
                            e.Ignore();
                        }
                    }
                }
            }
        }

        /// <remarks>
        /// <list type="bullet">
        /// <item>Upgrader from version 1.9.0-beta03 to 1.9.0-beta04 (PrefabAsset).</item>
        /// <item>Upgrader from version 1.9.0-beta04 to 1.9.0-beta05 (SceneAsset).</item>
        /// </list>
        /// </remarks>
        protected class MaterialFromModelComponentUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Parts;
                foreach (dynamic entityDesign in entities)
                {
                    var entity = entityDesign.Entity;
                    foreach (var component in entity.Components)
                    {
                        try
                        {
                            var componentTag = component.Value.Node.Tag;
                            if (componentTag == "!ModelComponent")
                            {
                                var materials = component.Value.Materials;
                                var node = ((DynamicYamlMapping)materials).Node;
                                var i = -1;
                                foreach (var material in node.Children.ToList())
                                {
                                    ++i;
                                    node.Children.Remove(material.Key);
                                    if (((YamlScalarNode)material.Value).Value == "null")
                                        continue;

                                    node.Children.Add(new YamlScalarNode(((YamlScalarNode)material.Key).Value + '~' + i), material.Value);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            try
                            {
                                // Component list serialized with the old version (as a sequence with ~Id in each item)
                                var componentTag = component.Node.Tag;
                                if (componentTag == "!ModelComponent")
                                {
                                    var materials = component.Materials;
                                    var node = ((DynamicYamlArray)materials).Node;
                                    var i = -1;
                                    dynamic newMaterial = new DynamicYamlMapping(new YamlMappingNode());
                                    foreach (var material in node.Children.ToList())
                                    {
                                        ++i;
                                        var reference = (YamlScalarNode)material;
                                        if (reference.Value == "null") // Skip null
                                            continue;

                                        UFile location;
                                        Guid referenceId;
                                        AssetId assetReference;
                                        if (AssetReference.TryParse(reference.Value, out assetReference, out location, out referenceId) && referenceId != Guid.Empty)
                                        {
                                            var itemId = new ItemId(referenceId.ToByteArray());
                                            newMaterial[itemId + "~" + i] = new AssetReference(assetReference, location);
                                        }
                                    }
                                    component["Materials"] = newMaterial;
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

        /// <summary>
        /// Upgrades a bug where Edge and Center for trails values were treated wrongly.
        /// </summary>
        /// <remarks>
        /// <item>Upgrader from version 1.9.0-beta04 to 1.9.0-beta05 (PrefabAsset).</item>
        /// <item>Upgrader from version 1.9.0-beta05 to 1.9.0-beta06 (SceneAsset).</item>
        /// </remarks>
        protected sealed class ParticleTrailEdgeUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Parts;
                foreach (dynamic entityDesign in entities)
                {
                    var entity = entityDesign.Entity;
                    foreach (var component in entity.Components)
                    {
                        try
                        {
                            var componentTag = component.Value.Node.Tag;
                            if (componentTag == "!ParticleSystemComponent")
                            {
                                dynamic particleSystem = component.Value.ParticleSystem;
                                if (particleSystem != null)
                                {
                                    foreach (dynamic emitter in particleSystem.Emitters)
                                    {
                                        dynamic shapeBuilder = emitter.Value.ShapeBuilder;
                                        if (shapeBuilder == null)
                                            continue;

                                        var shapeBuilderTag = shapeBuilder.Node.Tag;
                                        if (shapeBuilderTag != "!ShapeBuilderTrail")
                                            continue;

                                        if (shapeBuilder.EdgePolicy == "Center")
                                        {
                                            shapeBuilder["EdgePolicy"] = "Edge";
                                        }
                                        else
                                        {
                                            shapeBuilder["EdgePolicy"] = "Center";
                                        }
                                    }

                                }
                            }
                        }
                        catch (Exception)
                        {
                            try
                            {
                                var componentTag = component.Node.Tag;
                                if (componentTag == "!ParticleSystemComponent")
                                {
                                    dynamic particleSystem = component.ParticleSystem;
                                    if (particleSystem != null)
                                    {
                                        foreach (dynamic emitter in particleSystem.Emitters)
                                        {
                                            dynamic shapeBuilder = emitter.ShapeBuilder;
                                            if (shapeBuilder == null)
                                                continue;

                                            var shapeBuilderTag = shapeBuilder.Node.Tag;
                                            if (shapeBuilderTag != "!ShapeBuilderTrail")
                                                continue;

                                            if (shapeBuilder.EdgePolicy == "Center")
                                            {
                                                shapeBuilder["EdgePolicy"] = "Edge";
                                            }
                                            else
                                            {
                                                shapeBuilder["EdgePolicy"] = "Center";
                                            }
                                        }

                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                // Changing the edge policy is non-critical update so skip it if exception is thrown
                                e.Ignore();
                            }
                        }
                    }
                }
            }
        }
    }
}
