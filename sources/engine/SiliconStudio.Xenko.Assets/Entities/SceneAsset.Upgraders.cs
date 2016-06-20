// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SharpYaml.Serialization;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Physics;
using SiliconStudio.Xenko.Rendering.Lights;

namespace SiliconStudio.Xenko.Assets.Entities
{
    // All upgraders for SceneAsset
    public partial class SceneAsset
    {
        /// <summary>
        /// Upgrader from version 0 to version 1.
        /// </summary>
        class RemoveSourceUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                if (asset.Source != null)
                    asset.Source = DynamicYamlEmpty.Default;
                if (asset.SourceHash != null)
                    asset.SourceHash = DynamicYamlEmpty.Default;
            }
        }

        /// <summary>
        /// Upgrader from version 1 to 2.
        /// </summary>
        class RemoveBaseUpgrader : IAssetUpgrader
        {
            public void Upgrade(AssetMigrationContext context, string dependencyName, PackageVersion currentVersion, PackageVersion targetVersion, YamlMappingNode yamlAssetNode, PackageLoadingAssetFile assetFile)
            {
                dynamic asset = new DynamicYamlMapping(yamlAssetNode);
                var baseBranch = asset[Asset.BaseProperty];
                if (baseBranch != null)
                    asset[BaseProperty] = DynamicYamlEmpty.Default;

                AssetUpgraderBase.SetSerializableVersion(asset, dependencyName, targetVersion);
            }
        }

        /// <summary>
        /// Upgrader from version 2 to 3.
        /// </summary>
        class RemoveModelDrawOrderUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
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

        /// <summary>
        /// Upgrader from version 3 to 4.
        /// </summary>
        class RenameSpriteProviderUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
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

        /// <summary>
        /// Upgrader from version 4 to 5.
        /// </summary>
        class RemoveSpriteExtrusionMethodUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
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

        /// <summary>
        /// Upgrader from version 5 to 6.
        /// </summary>
        class RemoveModelParametersUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
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

        /// <summary>
        /// Upgrader from version 6 to 7.
        /// </summary>
        class RemoveEnabledFromIncompatibleComponent : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
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

        /// <summary>
        /// Upgrader from version 7 to 8.
        /// </summary>
        class SceneIsNotEntityUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
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

        /// <summary>
        /// Upgrader from version 8 to 9.
        /// </summary>
        class ColliderShapeAssetOnlyUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
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

        /// <summary>
        /// Upgrader from version 9 to 10.
        /// </summary>
        class NoBox2DUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
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

        /// <summary>
        /// Upgrader from version 10 to 11.
        /// </summary>
        class RemoveShadowImportanceUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
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

        /// <summary>
        /// Upgrader from version 11 to 12.
        /// </summary>
        class NewElementLayoutUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
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

        /// <summary>
        /// Upgrader from version 12 to 13.
        /// </summary>
        class NewElementLayoutUpgrader2 : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
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

        /// <summary>
        /// Upgrader from version 13 to 14.
        /// </summary>
        private class RemoveGammaTransformUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
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

        /// <summary>
        /// Upgrader from version 14 to 15.
        /// </summary>
        class EntityDesignUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
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

        /// <summary>
        /// Upgrader from version 15 to 16.
        /// </summary>
        class NewElementLayoutUpgrader3 : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
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

        /// <summary>
        /// Upgrader from version 16 to 17.
        /// </summary>
        class NewElementLayoutUpgrader4 : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
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

        /// <summary>
        /// Upgrader from version 17 to 18.
        /// </summary>
        class RemoveSceneEditorCameraSettings : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                asset.Hierarchy.SceneSettings.EditorSettings.Camera = DynamicYamlEmpty.Default;
            }
        }

        /// <summary>
        /// Upgrader from version 0.0.18 to 1.5.0-alpha01.
        /// </summary>
        class ChangeSpriteColorTypeAndTriggerElementRemoved : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
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

        /// <summary>
        /// Upgrader from version 1.5.0-alpha01 to 1.5.0-alpha02.
        /// </summary>
        class MoveSceneSettingsToSceneAsset : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                // Move SceneSettings to SceneAsset outside the HierarchyData
                var sceneSettings = asset.Hierarchy.SceneSettings;
                asset.SceneSettings = sceneSettings;
                var assetYaml = (DynamicYamlMapping)asset.Hierarchy;
                assetYaml.RemoveChild("SceneSettings");
            }
        }

        /// <summary>
        /// Upgrader from version 1.5.0-alpha02 to 1.6.0-beta.
        /// </summary>
        class MigrateToNewComponents : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Entities;

                var mapEntityComponents = new Dictionary<string, Dictionary<string, Tuple<string, dynamic>>>();
                var newScriptComponentsPerEntity = new Dictionary<string, List<dynamic>>();
                var newPhysicsComponentsPerEntity = new Dictionary<string, List<dynamic>>();

                // Collect current order of known components
                // We will use this order to add components in order to the new component list
                var mapComponentOrder = new Dictionary<string, int>();
                var knownComponentTypes = SiliconStudio.Core.Extensions.TypeDescriptorExtensions.GetInheritedInstantiableTypes(typeof(EntityComponent));
                const int defaultComponentOrder = 2345;
                foreach (var knownComponent in knownComponentTypes)
                {
                    var componentName = knownComponent.GetCustomAttribute<DataContractAttribute>()?.Alias;
                    if (componentName == null)
                    {
                        continue;
                    }
                    var order = knownComponent.GetCustomAttribute<ComponentOrderAttribute>(true)?.Order ?? defaultComponentOrder;

                    mapComponentOrder[componentName] = order;
                }
                mapComponentOrder["ScriptComponent"] = 1000;

                // --------------------------------------------------------------------------------------------
                // 1) Collect all components ids and key/types, collect all scripts
                // --------------------------------------------------------------------------------------------
                foreach (dynamic entityAndDesign in entities)
                {
                    var entity = entityAndDesign.Entity;
                    var entityId = (string)entity.Id;
                    var newComponents = new Dictionary<string, Tuple<string, dynamic>>();
                    mapEntityComponents.Add(entityId, newComponents);

                    foreach (var component in entity.Components)
                    {
                        var componentKey = (string)component.Key;
                        if (componentKey == "ScriptComponent.Key")
                        {
                            var newScripts = new List<dynamic>();
                            newScriptComponentsPerEntity.Add(entityId, newScripts);

                            foreach (var script in component.Value.Scripts)
                            {
                                newScripts.Add(script);
                            }
                        }
                        else if (componentKey == "PhysicsComponent.Key")
                        {
                            var newPhysics = new List<dynamic>();
                            newPhysicsComponentsPerEntity.Add(entityId, newPhysics);

                            foreach (var newPhysic in component.Value.Elements)
                            {
                                newPhysics.Add(newPhysic);
                            }
                        }
                        else
                        {
                            componentKey = GetComponentNameFromKey(componentKey);
                            var componentValue = component.Value;
                            var componentId = (string)componentValue["~Id"];

                            newComponents.Add(componentKey, new Tuple<string, dynamic>(componentId, componentValue));
                        }
                    }
                }

                // --------------------------------------------------------------------------------------------
                // 2) Collect all components ids and key/types
                // --------------------------------------------------------------------------------------------
                foreach (dynamic entityAndDesign in entities)
                {
                    var entity = entityAndDesign.Entity;

                    var entityId = (string)entity.Id;
                    var newComponents = mapEntityComponents[entityId];

                    // Order components
                    var orderedComponents = newComponents.ToList();

                    // Convert scripts to ScriptComponents
                    List<dynamic> scripts;
                    if (newScriptComponentsPerEntity.TryGetValue(entityId, out scripts))
                    {
                        foreach (var component in scripts)
                        {
                            // Update Script to ScriptComponent
                            var componentId = (string)component.Id;
                            component.RemoveChild("Id");
                            component["~Id"] = componentId;

                            var componentNode = (DynamicYamlMapping)component;
                            var componentType = componentNode.Node.Tag.Substring(1);

                            orderedComponents.Add(new KeyValuePair<string, Tuple<string, dynamic>>(componentType, new Tuple<string, dynamic>(componentId, component)));
                        }
                    }

                    // Convert PhysicsElements to PhysicsComponents
                    List<dynamic> physics;
                    if (newPhysicsComponentsPerEntity.TryGetValue(entityId, out physics))
                    {
                        foreach (var component in physics)
                        {
                            // Update Script to ScriptComponent
                            var componentId = (string)component["~Id"];
                            var componentNode = (DynamicYamlMapping)component;
                            var componentType = componentNode.Node.Tag.Substring(1);
                            componentType = componentType.Replace("Element", "Component");

                            orderedComponents.Add(new KeyValuePair<string, Tuple<string, dynamic>>(componentType, new Tuple<string, dynamic>(componentId, component)));
                        }
                    }

                    // Order components
                    orderedComponents.Sort((left, right) =>
                    {
                        int leftOrder;
                        if (!mapComponentOrder.TryGetValue(left.Key, out leftOrder))
                        {
                            leftOrder = defaultComponentOrder;
                        }

                        int rightOrder;
                        if (!mapComponentOrder.TryGetValue(right.Key, out rightOrder))
                        {
                            rightOrder = defaultComponentOrder;
                        }

                        return leftOrder.CompareTo(rightOrder);
                    });


                    // Reset previous component mapping
                    entity.Components = new DynamicYamlArray(new YamlSequenceNode());

                    foreach (var item in orderedComponents)
                    {
                        var componentKey = item.Key;

                        var component = (DynamicYamlMapping)item.Value.Item2;
                        component.Node.Tag = "!" + componentKey;

                        // Fix any component references.
                        FixEntityComponentReferences(component, mapEntityComponents);

                        entity.Components.Add(item.Value.Item2);
                    }
                }

                // Fix also component references in the settings
                if (asset.SceneSettings != null)
                {
                    FixEntityComponentReferences(asset.SceneSettings, mapEntityComponents);
                }
            }

            private string GetComponentNameFromKey(string componentKey)
            {
                if (componentKey.EndsWith(".Key"))
                {
                    componentKey = componentKey.Substring(0, componentKey.Length - ".Key".Length);
                }
                return componentKey;
            }

            private DynamicYamlMapping FixEntityComponentReferences(dynamic item, Dictionary<string, Dictionary<string, Tuple<string, dynamic>>> maps)
            {
                // Go recursively into an object to fix anykind of EntityComponent references

                var mapping = item as DynamicYamlMapping;
                var array = item as DynamicYamlArray;

                // We have an EntityComponentLink, transform it to the new format
                // Entity: {Id: guid}             =>    Entity: {Id: guid}
                // Component: Component.Key       =>    Id: guid
                if (mapping != null && item.Entity is DynamicYamlMapping && item.Component != null && item.Entity.Id != null && mapping.Node.Children.Count == 2)
                {
                    var entity = item.Entity;
                    var entityId = (string)entity.Id;
                    var componentKey = (string)item.Component;
                    var newComponentReference = new DynamicYamlMapping(new YamlMappingNode());
                    var newComponentDynamic = (dynamic)newComponentReference;

                    newComponentDynamic.Entity = entity;

                    string componentId = Guid.Empty.ToString();

                    Dictionary<string, Tuple<string, dynamic>> componentInfo;
                    if (maps.TryGetValue(entityId, out componentInfo))
                    {
                        var componentTypeName = GetComponentNameFromKey(componentKey);

                        Tuple<string, dynamic> newIdAndComponent;
                        if (componentInfo.TryGetValue(componentTypeName, out newIdAndComponent))
                        {
                            componentId = newIdAndComponent.Item1;
                        }

                        newComponentReference.Node.Tag = "!" + componentTypeName;
                        newComponentDynamic.Id = componentId;

                        return newComponentReference;
                    }
                    return null;
                }
                else if (mapping != null)
                {
                    foreach (var subKeyValue in mapping.Cast<KeyValuePair<dynamic, dynamic>>())
                    {
                        var newRef = FixEntityComponentReferences(subKeyValue.Value, maps);
                        if (newRef != null)
                        {
                            item[subKeyValue.Key] = newRef;
                        }
                    }
                }
                else if (array != null)
                {
                    var elements = array.Cast<dynamic>().ToList();
                    for (int i = 0; i < elements.Count; i++)
                    {
                        var arrayItem = elements[i];
                        var newRef = FixEntityComponentReferences(arrayItem, maps);
                        if (newRef != null)
                        {
                            item[i] = newRef;
                        }
                    }
                }

                // Otherwise we are not modifying anything
                return null;
            }
        }

        /// <summary>
        /// Upgrader from version 1.6.0-beta to 1.6.0-beta01.
        /// </summary>
        /// <remarks>
        /// Upgrades inconsistent Float and Float2 Min/Max fields into Float2 and Float4 MinMax fields respectively
        /// </remarks>
        class ParticleMinMaxFieldsUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {

                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Entities;
                foreach (dynamic entityAndDesign in entities)
                {
                    var entity = entityAndDesign.Entity;

                    foreach (var component in entity.Components)
                    {
                        var componentTag = component.Node.Tag;
                        if (componentTag == "!ParticleSystemComponent")
                        {
                            dynamic particleSystem = component.ParticleSystem;
                            if (particleSystem != null)
                            {

                                foreach (dynamic emitter in particleSystem.Emitters)
                                {
                                    // Lifetime changed from: float (ParticleMinLifetime), float (ParticleMaxLifetime) -> Vector2 (ParticleLifetime)
                                    dynamic lifetime = new DynamicYamlMapping(new YamlMappingNode());
                                    lifetime.AddChild("X", emitter.ParticleMinLifetime);
                                    lifetime.AddChild("Y", emitter.ParticleMaxLifetime);

                                    emitter.AddChild("ParticleLifetime", lifetime);

                                    emitter.RemoveChild("ParticleMinLifetime");
                                    emitter.RemoveChild("ParticleMaxLifetime");

                                    // Initializers
                                    foreach (dynamic initializer in emitter.Initializers)
                                    {
                                        var initializerTag = initializer.Node.Tag;
                                        if (initializerTag == "!InitialRotationSeed")
                                        {
                                            dynamic angle = new DynamicYamlMapping(new YamlMappingNode());
                                            angle.AddChild("X", initializer.AngularRotationMin);
                                            angle.AddChild("Y", initializer.AngularRotationMax);

                                            initializer.AddChild("AngularRotation", angle);

                                            initializer.RemoveChild("AngularRotationMin");
                                            initializer.RemoveChild("AngularRotationMax");

                                        }
                                    }

                                    // Updaters
                                    foreach (dynamic updater in emitter.Updaters)
                                    {
                                        var updaterTag = updater.Node.Tag;
                                        if (updaterTag == "!UpdaterCollider")
                                        {
                                            var isSolid = (bool)updater.IsSolid;

                                            updater.AddChild("IsHollow", !isSolid);

                                            updater.RemoveChild("IsSolid");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Upgrader from version 1.6.0-beta01 to 1.6.0-beta02.
        /// </summary>
        class ModelEffectUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                // TODO: Asset upgraders are called for BaseParts too, which might not be of the same type. Upgraders should be aware of this.
                if (asset.Node.Tag != "!SceneAsset")
                    return;

                var graphicsCompositor = asset.SceneSettings.GraphicsCompositor;

                if (graphicsCompositor != null && graphicsCompositor.Node.Tag == "!SceneGraphicsCompositorLayers")
                {
                    string modelEffect = null;

                    if (graphicsCompositor.Layers != null)
                    {
                        foreach (var layer in graphicsCompositor.Layers)
                        {
                            var layerModelEffect = RemoveModelEffectsFromLayer(layer);
                            if (modelEffect == null)
                            {
                                modelEffect = layerModelEffect;
                            }
                        }
                    }

                    if (graphicsCompositor.Master != null)
                    {
                        var layerModelEffect = RemoveModelEffectsFromLayer(graphicsCompositor.Master);
                        if (modelEffect == null)
                        {
                            modelEffect = layerModelEffect;
                        }
                    }

                    if (modelEffect != null)
                    {
                        graphicsCompositor.ModelEffect = modelEffect;
                    }
                }
            }

            private string RemoveModelEffectsFromLayer(dynamic layer)
            {
                string modelEffect = null;

                if (layer.Renderers != null)
                {
                    foreach (var renderer in layer.Renderers)
                    {
                        if (renderer.Node.Tag != "!SceneCameraRenderer")
                            continue;

                        var mode = renderer.Mode;
                        if (mode != null && mode.Node.Tag == "!CameraRendererModeForward" && mode.ModelEffect != null)
                        {
                            if (modelEffect == null)
                            {
                                modelEffect = mode.ModelEffect;
                            }

                            mode.RemoveChild("ModelEffect");
                        }
                    }
                }

                return modelEffect;
            }
        }

        /// <summary>
        /// Upgrader from version 1.6.0-beta02 to 1.6.0-beta03.
        /// </summary>
        class PhysicsFiltersUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Entities;
                foreach (dynamic entityAndDesign in entities)
                {
                    var entity = entityAndDesign.Entity;
                    foreach (var component in entity.Components)
                    {
                        var componentTag = component.Node.Tag;
                        if (componentTag == "!StaticColliderComponent" ||
                            componentTag == "!CharacterComponent" ||
                            componentTag == "!RigidbodyComponent")
                        {
                            if (component.CollisionGroup == null || (string)component.CollisionGroup == "0")
                            {
                                component.CollisionGroup = CollisionFilterGroups.DefaultFilter;
                            }
                           
                            if (component.CanCollideWith == null || (string)component.CanCollideWith == "0")
                            {
                                component.CanCollideWith = CollisionFilterGroupFlags.AllFilter;
                            }
                        }
                    }
                }
            }
        }
    }
}
