// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Assets.Materials;
using SiliconStudio.Paradox.Assets.Materials.Nodes;
using SiliconStudio.Paradox.Assets.Materials.Processor.Visitors;
using SiliconStudio.Paradox.Assets.Model.Analysis;
using SiliconStudio.Paradox.Assets.Textures;
using SiliconStudio.Paradox.Data;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Data;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.EntityModel.Data;
using SiliconStudio.Paradox.Importer.Common;

namespace SiliconStudio.Paradox.Assets.Model
{
    public abstract class ModelAssetImporter : AssetImporterBase
    {
        private static readonly Type[] supportedTypes = { typeof(EntityAsset), typeof(ModelAsset), typeof(TextureAsset), typeof(MaterialAsset), typeof(AnimationAsset), typeof(CameraAsset), typeof(LightAsset) };

        public override AssetImporterParameters GetDefaultParameters(bool isForReImport)
        {
            return new AssetImporterParameters(supportedTypes);
        }

        /// <summary>
        /// Get the entity information.
        /// </summary>
        /// <param name="localPath">The path of the asset.</param>
        /// <param name="logger">The logger to use to log import message.</param>
        /// <returns>The EntityInfo.</returns>
        public abstract EntityInfo GetEntityInfo(UFile localPath, Logger logger);
        
        /// <summary>
        /// Imports the model.
        /// </summary>
        /// <param name="localPath">The path of the asset.</param>
        /// <param name="importParameters">The parameters used to import the model.</param>
        /// <returns>A collection of assets.</returns>
        public override IEnumerable<AssetItem> Import(UFile localPath, AssetImporterParameters importParameters)
        {
            var assetReferences = new List<AssetItem>();

            var entityInfo = GetEntityInfo(localPath, importParameters.Logger);

            var isImportingEntity = importParameters.IsTypeSelectedForOutput<EntityAsset>();

            var isImportingModel = importParameters.IsTypeSelectedForOutput<ModelAsset>() ||
                                   isImportingEntity;

            var isImportingMaterial = importParameters.IsTypeSelectedForOutput<MaterialAsset>() ||
                                      isImportingModel;

            var isImportingTexture = importParameters.IsTypeSelectedForOutput<TextureAsset>() ||
                                     isImportingMaterial;

            var isImportingCamera = importParameters.IsTypeSelectedForOutput<CameraAsset>();

            var isImportingLight = importParameters.IsTypeSelectedForOutput<LightAsset>();


            // 1. Textures
            if (isImportingTexture)
            {
                ImportTextures(assetReferences, localPath, entityInfo.TextureDependencies);
            }

            // 2. Animation
            if (importParameters.IsTypeSelectedForOutput<AnimationAsset>())
            {
                ImportAnimation(assetReferences, localPath, entityInfo.AnimationNodes);
            }

            // 3. Materials
            if (isImportingMaterial)
            {
                ImportMaterials(assetReferences, localPath, entityInfo.Materials);
            }

            // 4. Model
            if (isImportingModel)
            {
                var modelItem = ImportModel(assetReferences, localPath, localPath, entityInfo);

                // 4. Entity
                if (isImportingEntity)
                {
                    var entityAssetItem = ImportEntity(assetReferences, localPath, modelItem);
                    var entityAsset = (EntityAsset)entityAssetItem.Asset;
                    var rootEntityData = entityAsset.Hierarchy.Entities[entityAsset.Hierarchy.RootEntity];

                    // 5. Camera
                    if (isImportingCamera)
                        ImportCameras(entityInfo, rootEntityData, (ModelAsset)modelItem.Asset);

                    // 6. Lights
                    if (isImportingLight)
                        ImportLights(entityInfo, rootEntityData, (ModelAsset)modelItem.Asset);

                    // Apply EntityAnalysis 
                    EntityAnalysis.UpdateEntityReferences(((EntityAsset)entityAssetItem.Asset).Hierarchy);
                }
            }

            return assetReferences;
        }

        private static void ImportLights(EntityInfo entityInfo, EntityData rootEntityData, ModelAsset modelAsset)
        {
            if (entityInfo.Lights == null)
                return;

            foreach (var light in entityInfo.Lights)
            {
                var cameraEntityAsset = CreateTrackingEntity(rootEntityData, modelAsset, light.NodeName);
                cameraEntityAsset.Components.Add(LightComponent.Key, light.Data);
            }
        }

        private static void ImportCameras(EntityInfo entityInfo, EntityData rootEntityData, ModelAsset modelAsset)
        {
            if (entityInfo.Cameras == null)
                return;

            foreach (var camera in entityInfo.Cameras)
            {
                var cameraEntityAsset = CreateTrackingEntity(rootEntityData, modelAsset, camera.NodeName);
                cameraEntityAsset.Components.Add(CameraComponent.Key, camera.Data);

                if (camera.TargetNodeName != null)
                {
                    // We have a target, create an entity for it
                    var cameraTargetEntityAsset = CreateTrackingEntity(rootEntityData, modelAsset, camera.TargetNodeName);

                    // Update target
                    camera.Data.Target = new EntityReference { Value = cameraTargetEntityAsset };
                }
            }
        }

        private static EntityData CreateTrackingEntity(EntityData rootEntityAsset, ModelAsset modelAsset, string nodeName)
        {
            var childEntity = new EntityData { Name = nodeName };

            // Add TransformationComponent
            childEntity.Components.Add(TransformationComponent.Key, new TransformationComponentData());

            // Add ModelNodeLinkComponent
            childEntity.Components.Add(ModelNodeLinkComponent.Key, new ModelNodeLinkComponentData
            {
                NodeName = nodeName,
                Target = EntityComponentReference.New<ModelComponent>(rootEntityAsset.Components[ModelComponent.Key]),
            });

            // Add this asset to the list
            rootEntityAsset.Container.Entities.Add(childEntity);

            // Get or create transformation component
            EntityComponentData entityComponentData;
            if (!rootEntityAsset.Components.TryGetValue(TransformationComponent.Key, out entityComponentData))
            {
                entityComponentData = new TransformationComponentData();
                rootEntityAsset.Components.Add(TransformationComponent.Key, entityComponentData);
            }

            // Mark node as preserved
            modelAsset.PreserveNodes(new List<string> { nodeName });

            // Add as children of model entity
            ((TransformationComponentData)entityComponentData).Children.Add(
                EntityComponentReference.New<TransformationComponent>(childEntity.Components[TransformationComponent.Key]));

            return childEntity;
        }

        private static AssetItem ImportEntity(List<AssetItem> assetReferences, UFile localPath, AssetItem modelItem)
        {
            var entityUrl = new UFile(localPath.GetFileName(), null);

            var asset = new EntityAsset { Source = localPath };
            var rootEntityData = new EntityData();
            asset.Hierarchy.Entities.Add(rootEntityData);
            asset.Hierarchy.RootEntity = rootEntityData.Id;

            rootEntityData.Name = entityUrl;
            // Use modelUrl.Path to get the url without the extension
            rootEntityData.Components.Add(ModelComponent.Key, new ModelComponentData { Model = new ContentReference<ModelData>(modelItem.Id, modelItem.Location), Enabled = true });

            var assetReference = new AssetItem(entityUrl, asset);
            assetReferences.Add(assetReference);

            return assetReference;
        }

        private static void ImportAnimation(List<AssetItem> assetReferences, UFile localPath, List<string> animationNodes)
        {
            if (animationNodes != null && animationNodes.Count > 0)
            {
                var assetSource = localPath;
                var animUrl = new UFile(localPath.GetFileName() + "_anim", null);

                var asset = new AnimationAsset { Source = assetSource };

                assetReferences.Add(new AssetItem(animUrl, asset));
            }
        }

        private static AssetItem ImportModel(List<AssetItem> assetReferences, UFile assetSource, UFile localPath, EntityInfo entityInfo)
        {
            var frontAxis = Vector3.Cross(entityInfo.UpAxis, Vector3.UnitZ).Length() < MathUtil.ZeroTolerance ? Vector3.UnitY : Vector3.UnitZ;
            var asset = new ModelAsset { Source = assetSource, UpAxis = entityInfo.UpAxis, FrontAxis = frontAxis };

            if (entityInfo.Models != null)
            {
                var loadedMaterials = assetReferences.Where(x => x.Asset is MaterialAsset).ToList();
                foreach (var material in entityInfo.Materials)
                {
                    var matName = GenerateFinalMaterialName(localPath, material.Key);
                    var foundMaterial = loadedMaterials.FirstOrDefault(x => x.Location == new UFile(matName, null));
                    if (foundMaterial != null)
                        asset.Materials.Add(new ModelMaterial { Name = material.Key, Material = new AssetReference<MaterialAsset>(foundMaterial.Id, foundMaterial.Location) });
                }
            }

            if (entityInfo.Nodes != null)
            {
                foreach (var node in entityInfo.Nodes)
                    asset.Nodes.Add(new NodeInformation(node.Name, node.Depth, node.Preserve));
            }

            if (entityInfo.AnimationNodes != null && entityInfo.AnimationNodes.Count > 0)
                asset.PreserveNodes(entityInfo.AnimationNodes);

            var modelUrl = new UFile(localPath.GetFileName() + "_model", null);
            var assetItem = new AssetItem(modelUrl, asset);
            assetReferences.Add(assetItem);
            return assetItem;
        }

        private static void ImportMaterials(List<AssetItem> assetReferences, UFile localPath, Dictionary<string, MaterialDescription> materials)
        {
            if (materials != null)
            {
                var assetSource = localPath;
                var loadedTextures = assetReferences.Where(x => x.Asset is TextureAsset).ToList();

                foreach (var materialKeyValue in materials)
                {
                    AdjustForTransparency(materialKeyValue.Value);
                    var material = materialKeyValue.Value;
                    var materialUrl = new UFile(GenerateFinalMaterialName(localPath, materialKeyValue.Key), null);
                    var asset = new MaterialAsset { Material = material };

                    // patch texture name and ids
                    var textureVisitor = new MaterialTextureVisitor(material);
                    var textures = textureVisitor.GetAllTextureValues();
                    foreach (var texture in textures)
                    {
                        // texture location is #nameOfTheModel_#nameOfTheTexture at this point in the material
                        var foundTexture = loadedTextures.FirstOrDefault(x => x.Location == GenerateFinalTextureUrl(localPath, texture.TextureReference.Location));
                        if (foundTexture != null)
                            texture.TextureReference = new AssetReference<TextureAsset>(foundTexture.Id, foundTexture.Location);
                    }

                    var assetReference = new AssetItem(materialUrl, asset);
                    assetReferences.Add(assetReference);
                }
            }
        }

        /// <summary>
        /// Modify the material to comply with its transparency parameters.
        /// </summary>
        /// <param name="material">The material/</param>
        private static void AdjustForTransparency(MaterialDescription material)
        {
            // Note: at this point, there is no other nodes than diffuse, specular, transparent, normal and displacement
            if (material.ColorNodes.ContainsKey(MaterialParameters.AlbedoDiffuse))
            {
                var diffuseNode = material.GetMaterialNode(MaterialParameters.AlbedoDiffuse);
                if (material.ColorNodes.ContainsKey(MaterialParameters.TransparencyMap))
                {
                    var diffuseNodeName = material.ColorNodes[MaterialParameters.AlbedoDiffuse];
                    var transparentNodeName = material.ColorNodes[MaterialParameters.TransparencyMap];

                    var transparentNode = material.GetMaterialNode(MaterialParameters.TransparencyMap);

                    if (diffuseNode == null || transparentNode == null)
                        return;

                    var foundTextureDiffuse = FindTextureNode(material, diffuseNodeName);
                    var foundTextureTransparent = FindTextureNode(material, transparentNodeName);

                    if (foundTextureDiffuse != null && foundTextureTransparent != null)
                    {
                        if (foundTextureDiffuse != foundTextureTransparent)
                        {
                            var alphaMixNode = new MaterialBinaryNode(diffuseNode, transparentNode, MaterialBinaryOperand.SubstituteAlpha);
                            material.AddColorNode(MaterialParameters.AlbedoDiffuse, "pdx_diffuseWithAlpha", alphaMixNode);
                        }
                    }

                    // set the key if it was missing
                    material.Parameters.Set(MaterialParameters.UseTransparent, true);
                }
                else
                {
                    // NOTE: MaterialParameters.UseTransparent is mostly runtime
                    var isTransparent = false;
                    if (material.Parameters.ContainsKey(MaterialParameters.UseTransparent))
                        isTransparent = (bool)material.Parameters[MaterialParameters.UseTransparent];
                    
                    if (!isTransparent)
                    {
                        // remove the diffuse node
                        var diffuseName = material.ColorNodes[MaterialParameters.AlbedoDiffuse];
                        material.Nodes.Remove(diffuseName);

                        // add the new one
                        var opaqueNode = new MaterialBinaryNode(diffuseNode, null, MaterialBinaryOperand.Opaque);
                        material.AddColorNode(MaterialParameters.AlbedoDiffuse, "pdx_diffuseOpaque", opaqueNode);
                    }
                }
            }
        }

        /// <summary>
        /// Explore the material to find a MaterialTextureNode behind a name.
        /// </summary>
        /// <param name="material">The material.</param>
        /// <param name="startNode">The name of the stating node.</param>
        /// <returns>The MaterialTextureNode if found.</returns>
        private static MaterialTextureNode FindTextureNode(MaterialDescription material, string startNode)
        {
            var currentNode = material.FindNode(startNode);
            while (currentNode is MaterialReferenceNode)
            {
                var currentReferenceNode = (MaterialReferenceNode)currentNode;
                currentNode = material.FindNode(currentReferenceNode.Name);
            }

            return currentNode as MaterialTextureNode;
        }

        private static string GenerateFinalMaterialName(UFile localPath, string materialId)
        {
            return localPath.GetFileName() + "_material_" + materialId;
        }

        private static UFile GenerateFinalTextureUrl(UFile localPath, string textureName)
        {
            return new UFile(localPath.GetFileName() + '_' + textureName, null);
        }

        private static void ImportTextures(List<AssetItem> assetReferences, UFile localPath, List<string> dependentTextures)
        {
            if (dependentTextures != null)
            {
                // Import each texture
                foreach (var textureFullPath in dependentTextures.Distinct(x => x))
                {
                    ImportTexture(assetReferences, localPath, textureFullPath);
                }
            }
        }

        private static void ImportTexture(List<AssetItem> assetReferences, UFile localPath, string textureFullPath)
        {
            var texturePath = new UFile(textureFullPath);

            var source = texturePath;
            var texture = new TextureAsset { Source = source, PremultiplyAlpha = false };

            // Creates the url to the texture
            var textureUrl = GenerateFinalTextureUrl(localPath, texturePath.GetFileName());

            // Create asset reference
            assetReferences.Add(new AssetItem(textureUrl, texture));
        }

        /// <summary>
        /// Used only for category purpose, there is no such thing as a Camera asset (it will be a CameraComponent inside an EntityAsset).
        /// </summary>
        [Display("Camera", "A camera")]
        class CameraAsset : Asset
        {

        }

        /// <summary>
        /// Used only for category purpose, there is no such thing as a Light asset (it will be a LightComponent inside an EntityAsset).
        /// </summary>
        [Display("Light", "A light")]
        class LightAsset : Asset
        {

        }
    }
}
