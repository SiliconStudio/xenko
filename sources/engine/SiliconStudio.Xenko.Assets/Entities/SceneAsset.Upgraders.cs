// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SharpYaml.Serialization;
using SiliconStudio.Assets;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Rendering.Lights;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public partial class SceneAsset
    {
        // All upgraders for SceneAsset
        class RemoveSourceUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                if (asset.Source != null)
                    asset.Source = DynamicYamlEmpty.Default;
                if (asset.SourceHash != null)
                    asset.SourceHash = DynamicYamlEmpty.Default;
            }
        }

        class RemoveBaseUpgrader : IAssetUpgrader
        {
            public void Upgrade(AssetMigrationContext context, string dependencyName, PackageVersion currentVersion, PackageVersion targetVersion, YamlMappingNode yamlAssetNode, PackageLoadingAssetFile assetFile)
            {
                dynamic asset = new DynamicYamlMapping(yamlAssetNode);
                var baseBranch = asset["~Base"];
                if (baseBranch != null)
                    asset["~Base"] = DynamicYamlEmpty.Default;

                AssetUpgraderBase.SetSerializableVersion(asset, dependencyName, targetVersion);
            }
        }

        class RemoveModelDrawOrderUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Entities;
                foreach (dynamic entity in entities)
                {
                    var components = entity.Components;
                    var modelComponent = components["ModelComponent.Key"];
                    if (modelComponent != null)
                        modelComponent.RemoveChild("DrawOrder");
                }
            }
        }

        class RenameSpriteProviderUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Entities;
                foreach (dynamic entity in entities)
                {
                    var components = entity.Components;
                    var spriteComponent = components["SpriteComponent.Key"];
                    if (spriteComponent != null)
                    {
                        var provider = spriteComponent.SpriteProvider;
                        var providerAsMap = provider as DynamicYamlMapping;
                        if (providerAsMap != null && providerAsMap.Node.Tag == "!SpriteFromSpriteGroup")
                        {
                            provider.Sheet = provider.SpriteGroup;
                            provider.SpriteGroup = DynamicYamlEmpty.Default;
                            providerAsMap.Node.Tag = "!SpriteFromSheet";
                        }
                    }
                }
            }
        }

        class RemoveSpriteExtrusionMethodUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Entities;
                foreach (dynamic entity in entities)
                {
                    var components = entity.Components;
                    var spriteComponent = components["SpriteComponent.Key"];
                    if (spriteComponent != null)
                        spriteComponent.RemoveChild("ExtrusionMethod");
                }
            }
        }

        class RemoveModelParametersUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Entities;
                foreach (dynamic entity in entities)
                {
                    var components = entity.Components;
                    var spriteComponent = components["ModelComponent.Key"];
                    if (spriteComponent != null)
                        spriteComponent.RemoveChild("Parameters");
                }
            }
        }

        class RemoveEnabledFromIncompatibleComponent : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Entities;
                foreach (dynamic entity in entities)
                {
                    foreach (var component in entity.Components)
                    {
                        // All components not in this list won't have Enabled anymore
                        if (component.Key != "BackgroundComponent.Key"
                            && component.Key != "CameraComponent.Key"
                            && component.Key != "ChildSceneComponent.Key"
                            && component.Key != "LightComponent.Key"
                            && component.Key != "ModelComponent.Key"
                            && component.Key != "SkyboxComponent.Key"
                            && component.Key != "SpriteComponent.Key"
                            && component.Key != "UIComponent.Key")
                        {
                            component.Value.RemoveChild("Enabled");
                        }
                    }
                }
            }
        }

        class SceneIsNotEntityUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                // Transform RootEntity in RootEntities
                var rootEntityFieldIndex = asset.Hierarchy.IndexOf("RootEntity");
                asset.Hierarchy.RootEntity = DynamicYamlEmpty.Default;

                asset.Hierarchy.RootEntities = new DynamicYamlArray(new YamlSequenceNode());

                // Make sure it is at same position than just removed RootEntity
                asset.Hierarchy.MoveChild("RootEntities", rootEntityFieldIndex);

                // Remove previous root entity and make its SceneComponent as Hierarchy.SceneComponent
                int entityIndex = 0;
                dynamic rootEntity = null;
                foreach (var entity in asset.Hierarchy.Entities)
                {
                    if (entity.Node.Tag == "!Scene")
                    {
                        // Capture root entity and delete it from this list
                        rootEntity = entity;
                        asset.Hierarchy.Entities.RemoveAt(entityIndex);
                        break;
                    }
                    entityIndex++;
                }

                if (rootEntity == null)
                {
                    throw new InvalidOperationException("Could not upgrade SceneAsset because there no root Scene could be found");
                }

                // Update list of root entities
                foreach (var child in rootEntity.Components["TransformComponent.Key"].Children)
                {
                    asset.Hierarchy.RootEntities.Add(child.Entity.Id);
                }

                // Move scene component
                asset.Hierarchy.SceneSettings = rootEntity.Components["SceneComponent.Key"];
            }
        }

        class ColliderShapeAssetOnlyUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Entities;
                foreach (dynamic entity in entities)
                {
                    var components = entity.Components;
                    var physComponent = components["PhysicsComponent.Key"];
                    if (physComponent != null)
                    {
                        foreach (dynamic element in physComponent.Elements)
                        {
                            var index = element.IndexOf("Shape");
                            if (index == -1) continue;

                            dynamic shapeId = element.Shape;
                            element.ColliderShapes = new DynamicYamlArray(new YamlSequenceNode());
                            dynamic subnode = new YamlMappingNode { Tag = "!ColliderShapeAssetDesc" };
                            subnode.Add("Shape", shapeId.Node.Value);
                            element.ColliderShapes.Add(subnode);

                            element.RemoveChild("Shape");
                        }
                    }
                }
            }
        }

        class NoBox2DUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Entities;
                foreach (dynamic entity in entities)
                {
                    var components = entity.Components;
                    var physComponent = components["PhysicsComponent.Key"];
                    if (physComponent != null)
                    {
                        foreach (dynamic element in physComponent.Elements)
                        {
                            foreach (dynamic shape in element.ColliderShapes)
                            {
                                var tag = shape.Node.Tag;
                                if (tag == "!Box2DColliderShapeDesc")
                                {
                                    shape.Node.Tag = "!BoxColliderShapeDesc";
                                    shape.Is2D = true;
                                    shape.Size.X = shape.Size.X;
                                    shape.Size.Y = shape.Size.Y;
                                    shape.Size.Z = 0.01f;
                                }
                            }
                        }
                    }
                }
            }
        }

        class RemoveShadowImportanceUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Entities;
                foreach (dynamic entity in entities)
                {
                    var components = entity.Components;
                    var lightComponent = components["LightComponent.Key"];
                    if (lightComponent != null)
                    {
                        var lightType = lightComponent.Type;
                        if (lightType != null)
                        {
                            var shadow = lightType.Shadow;
                            if (shadow != null)
                            {
                                var size = (OldLightShadowMapSize)(shadow.Size ?? OldLightShadowMapSize.Small);
                                var importance = (OldLightShadowImportance)(shadow.Importance ?? OldLightShadowImportance.Low);

                                // Convert back the old size * importance to the new size
                                var factor = importance == OldLightShadowImportance.High ? 2.0 : importance == OldLightShadowImportance.Medium ? 1.0 : 0.5;
                                factor *= Math.Pow(2.0, (int)size - 2.0);
                                var value = ((int)Math.Log(factor, 2.0)) + 3;

                                var newSize = (LightShadowMapSize)Enum.ToObject(typeof(LightShadowMapSize), value);
                                shadow.Size = newSize;

                                shadow.RemoveChild("Importance");
                            }
                        }
                    }
                }
            }

            private enum OldLightShadowMapSize
            {
                Small,
                Medium,
                Large
            }


            private enum OldLightShadowImportance
            {
                Low,
                Medium,
                High
            }
        }

        class NewElementLayoutUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Entities;
                foreach (dynamic entity in entities)
                {
                    var components = entity.Components;
                    var physComponent = components["PhysicsComponent.Key"];
                    if (physComponent != null)
                    {
                        foreach (dynamic element in physComponent.Elements)
                        {
                            var type = element.Type.Node.Value;

                            if (type == "PhantomCollider")
                            {
                                element.Node.Tag = "!TriggerElement";
                                element.RemoveChild("StepHeight");
                            }
                            else if (type == "StaticCollider")
                            {
                                element.Node.Tag = "!StaticColliderElement";
                                element.RemoveChild("StepHeight");
                            }
                            else if (type == "StaticRigidBody")
                            {
                                element.Node.Tag = "!StaticRigidbodyElement";
                                element.RemoveChild("StepHeight");
                            }
                            else if (type == "DynamicRigidBody")
                            {
                                element.Node.Tag = "!DynamicRigidbodyElement";
                                element.RemoveChild("StepHeight");
                            }
                            else if (type == "KinematicRigidBody")
                            {
                                element.Node.Tag = "!KinematicRigidbodyElement";
                                element.RemoveChild("StepHeight");
                            }
                            else if (type == "CharacterController")
                            {
                                element.Node.Tag = "!CharacterElement";
                            }

                            element.RemoveChild("Type");
                        }
                    }
                }
            }
        }

        class NewElementLayoutUpgrader2 : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Entities;
                foreach (dynamic entity in entities)
                {
                    var components = entity.Components;
                    var physComponent = components["PhysicsComponent.Key"];
                    if (physComponent != null)
                    {
                        foreach (dynamic element in physComponent.Elements)
                        {
                            if (element.Node.Tag == "!TriggerElement" ||
                                element.Node.Tag == "!StaticColliderElement" ||
                                element.Node.Tag == "!StaticRigidbodyElement" ||
                                element.Node.Tag == "!CharacterElement"
                                )
                            {
                                element.RemoveChild("LinkedBoneName");
                            }
                        }
                    }
                }
            }
        }

        private class RemoveGammaTransformUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                var hierarchy = asset.Hierarchy;

                // Remove from all layers/renderers
                var layers = (DynamicYamlArray)hierarchy?.SceneSettings?.GraphicsCompositor?.Layers;
                if (layers != null)
                {
                    foreach (dynamic layer in layers)
                    {
                        ProcessRenderers((DynamicYamlArray)layer.Renderers);
                    }
                }

                var masterRenderers = (DynamicYamlArray)hierarchy?.SceneSettings?.GraphicsCompositor?.Master?.Renderers;
                ProcessRenderers(masterRenderers);

                // Remove from editor settings
                var colorTransforms = hierarchy?.SceneSettings?.EditorSettings?.Mode?.PostProcessingEffects?.ColorTransforms;
                if (colorTransforms != null)
                {
                    colorTransforms.RemoveChild("GammaTransform");

                    // Because the color was stored in linear, we need to store it back to gamma 
                    // We also apply a x2 to the color to 
                    var color = hierarchy.SceneSettings.EditorSettings.Mode.BackgroundColor;
                    if (color != null)
                    {
                        color["R"] = MathUtil.Clamp(MathUtil.LinearToSRgb((float)color["R"]) * 2.0f, 0.0f, 1.0f);
                        color["G"] = MathUtil.Clamp(MathUtil.LinearToSRgb((float)color["G"]) * 2.0f, 0.0f, 1.0f);
                        color["B"] = MathUtil.Clamp(MathUtil.LinearToSRgb((float)color["B"]) * 2.0f, 0.0f, 1.0f);
                    }
                }
            }

            void ProcessRenderers(DynamicYamlArray renderers)
            {
                foreach (dynamic renderer in renderers)
                {
                    var colorTransforms = renderer.Effect?.ColorTransforms;
                    if (colorTransforms != null)
                    {
                        colorTransforms.RemoveChild("GammaTransform");
                    }
                }
            }
        }

        class EntityDesignUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                var entities = asset.Hierarchy.Entities;
                var designEntities = new YamlSequenceNode();
                asset.Hierarchy.Entities = designEntities;

                foreach (var entity in entities)
                {
                    var designEntity = new YamlMappingNode();
                    dynamic dynamicDesignEntity = new DynamicYamlMapping(designEntity);
                    dynamicDesignEntity.Entity = entity;
                    designEntities.Add(designEntity);
                }
            }
        }

        class NewElementLayoutUpgrader3 : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Entities;
                foreach (dynamic entity in entities)
                {
                    var components = entity.Entity.Components;
                    var physComponent = components["PhysicsComponent.Key"];
                    if (physComponent == null) continue;

                    foreach (dynamic element in physComponent.Elements)
                    {
                        if (element.Node.Tag == "!TriggerElement")
                        {
                            element.RemoveChild("LinkedBoneName");
                        }
                        else if (element.Node.Tag == "!KinematicRigidbodyElement")
                        {
                            element.Node.Tag = "!RigidbodyElement";
                            element.IsKinematic = true;
                        }
                        else if (element.Node.Tag == "!DynamicRigidbodyElement")
                        {
                            element.Node.Tag = "!RigidbodyElement";
                            element.IsKinematic = false;
                        }
                        else if (element.Node.Tag == "!StaticRigidbodyElement")
                        {
                            element.Node.Tag = "!StaticColliderElement";
                        }

                        element.ProcessCollisions = true;
                    }
                }
            }
        }

        class NewElementLayoutUpgrader4 : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Entities;
                foreach (dynamic entity in entities)
                {
                    var components = entity.Entity.Components;
                    var physComponent = components["PhysicsComponent.Key"];
                    if (physComponent == null) continue;

                    foreach (dynamic element in physComponent.Elements)
                    {
                        if (element.Node.Tag == "!RigidbodyElement")
                        {
                            element.NodeName = element.LinkedBoneName;
                            element.RemoveChild("LinkedBoneName");
                        }
                    }
                }
            }
        }

        class RemoveSceneEditorCameraSettings : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                asset.Hierarchy.SceneSettings.EditorSettings.Camera = DynamicYamlEmpty.Default;
            }
        }


        class ChangeSpriteColorTypeAndTriggerElementRemoved : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                // SerializedVersion format changed during renaming upgrade. However, before this was merged back in master, some asset upgrader still with older version numbers were developed.
                // As a result, sprite component upgrade is not needed for version 19 and 20, and physics component upgrade is not needed for version 20
                var version19 = PackageVersion.Parse("0.0.19");
                var version20 = PackageVersion.Parse("0.0.20");

                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Entities;
                foreach (dynamic entity in entities)
                {
                    var components = entity.Entity.Components;
                    var spriteComponent = components["SpriteComponent.Key"];
                    if (spriteComponent != null && (currentVersion != version19 && currentVersion != version20))
                    {
                        var color = spriteComponent.Color;
                        spriteComponent.Color = DynamicYamlExtensions.ConvertFrom((Color4)DynamicYamlExtensions.ConvertTo<Color>(color));
                    }

                    var physComponent = components["PhysicsComponent.Key"];
                    if (physComponent != null && (currentVersion != version20))
                    {
                        foreach (dynamic element in physComponent.Elements)
                        {
                            if (element.Node.Tag == "!TriggerElement")
                            {
                                element.Node.Tag = "!StaticColliderElement";
                                element.IsTrigger = true;
                            }
                        }
                    }
                }
            }
        }

        class MoveSceneSettingsToSceneAsset : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                // Move SceneSettings to SceneAsset outside the HierarchyData
                var sceneSettings = asset.Hierarchy.SceneSettings;
                asset.SceneSettings = sceneSettings;
                var assetYaml = (DynamicYamlMapping)asset.Hierarchy;
                assetYaml.RemoveChild("SceneSettings");
            }
        }
    }
}