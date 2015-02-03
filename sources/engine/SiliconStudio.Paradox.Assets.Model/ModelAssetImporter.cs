// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Assets;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Assets.Materials;
using SiliconStudio.Paradox.Assets.Materials.Nodes;
using SiliconStudio.Paradox.Assets.Materials.Processor.Visitors;
using SiliconStudio.Paradox.Assets.Textures;
using SiliconStudio.Paradox.Data;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Data;
using SiliconStudio.Paradox.EntityModel.Data;
using SiliconStudio.Paradox.Importer.Common;

namespace SiliconStudio.Paradox.Assets.Model
{
    public abstract class ModelAssetImporter : AssetImporterBase
    {
        private static readonly Type[] supportedTypes = { typeof(EntityAsset), typeof(ModelAsset), typeof(TextureAsset), typeof(MaterialAsset), typeof(AnimationAsset), typeof(CameraAsset), typeof(LightAsset) };

        public override AssetImporterParameters GetDefaultParameters(bool isForReImport)
        {
            var parameters = new AssetImporterParameters(supportedTypes);

            // When we are reimporting, we don't want the asset to be reimported by default
            if (isForReImport)
            {
                parameters.SelectedOutputTypes[typeof(EntityAsset)] = false;
            }
            return parameters;
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
                    var entityAsset = ImportEntity(assetReferences, localPath, modelItem, entityInfo);

                    // 5. Camera
                    if (isImportingCamera)
                        ImportCameras(assetReferences, localPath, entityInfo, entityAsset, modelItem);

                    // 6. Lights
                    if (isImportingLight)
                        ImportLights(assetReferences, localPath, entityInfo, entityAsset, modelItem);
                }
            }

            return assetReferences;
        }

        private static void ImportLights(List<AssetItem> assetReferences, UFile localPath, EntityInfo entityInfo, AssetItem entityAsset, AssetItem modelAsset)
        {
            if (entityInfo.Lights == null)
                return;

            foreach (var light in entityInfo.Lights)
            {
                var lightUrl = new UFile(localPath.GetFileName() + "_light_" + light.NodeName, null);

                var cameraEntityAsset = CreateTrackingEntity(entityAsset, modelAsset, lightUrl, light.NodeName);
                ((EntityAsset)cameraEntityAsset.Asset).Data.Components.Add(LightComponent.Key, light.Data);

                assetReferences.Add(cameraEntityAsset);
            }
        }

        private static void ImportCameras(List<AssetItem> assetReferences, UFile localPath, EntityInfo entityInfo, AssetItem entityAsset, AssetItem modelAsset)
        {
            if (entityInfo.Cameras == null)
                return;

            foreach (var camera in entityInfo.Cameras)
            {
                var cameraUrl = new UFile(localPath.GetFileName() + "_camera_" + camera.NodeName, null);

                var cameraEntityAsset = CreateTrackingEntity(entityAsset, modelAsset, cameraUrl, camera.NodeName);
                ((EntityAsset)cameraEntityAsset.Asset).Data.Components.Add(CameraComponent.Key, camera.Data);

                if (camera.TargetNodeName != null)
                {
                    // We have a target, create an entity for it
                    var cameraTargetUrl = new UFile(localPath.GetFileName() + "_cameratarget_" + camera.TargetNodeName, null);
                    var cameraTargetEntityAsset = CreateTrackingEntity(entityAsset, modelAsset, cameraTargetUrl, camera.TargetNodeName);

                    // Update target
                    camera.Data.Target = new ContentReference<EntityData>(cameraTargetEntityAsset.Id, cameraTargetEntityAsset.Location);

                    assetReferences.Add(cameraTargetEntityAsset);
                }

                assetReferences.Add(cameraEntityAsset);
            }
        }

        private static AssetItem CreateTrackingEntity(AssetItem rootEntityAsset, AssetItem modelAsset, UFile cameraUrl, string nodeName)
        {
            var entity = new EntityData { Name = nodeName };

            // Add TransformationComponent
            entity.Components.Add(TransformationComponent.Key, new TransformationComponentData());


            // Add ModelNodeLinkComponent
            entity.Components.Add(ModelNodeLinkComponent.Key, new ModelNodeLinkComponentData
            {
                NodeName = nodeName,
                Target = EntityComponentReference.New(rootEntityAsset.Id, rootEntityAsset.Location, ModelComponent.Key),
            });

            var asset = new EntityAsset { Data = entity };
            var entityAsset = new AssetItem(cameraUrl, asset);

            var parentEntity = (EntityAsset)rootEntityAsset.Asset;

            // Get or create transformation component
            EntityComponentData entityComponentData;
            if (!parentEntity.Data.Components.TryGetValue(TransformationComponent.Key, out entityComponentData))
            {
                entityComponentData = new TransformationComponentData();
                parentEntity.Data.Components.Add(TransformationComponent.Key, entityComponentData);
            }

            // Mark node as preserved
            ((ModelAsset)modelAsset.Asset).PreserveNodes(new List<string> { nodeName });

            // Add as children of model entity
            ((TransformationComponentData)entityComponentData).Children.Add(
                EntityComponentReference.New(entityAsset.Id, entityAsset.Location, TransformationComponent.Key));


            return entityAsset;
        }

        private static AssetItem ImportEntity(List<AssetItem> assetReferences, UFile localPath, AssetItem modelItem, EntityInfo entityInfo)
        {
            var entityUrl = new UFile(localPath.GetFileName(), null);

            var asset = new EntityAsset();
            asset.Data.Name = entityUrl;
            // Use modelUrl.Path to get the url without the extension
            asset.Data.Components.Add(ModelComponent.Key, new ModelComponentData { Model = new ContentReference<ModelData>(modelItem.Id, modelItem.Location), Enabled = true });

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
                foreach (var model in entityInfo.Models)
                {
                    var meshParams = new MeshMaterialParameters { Parameters = model.Parameters, NodeName = model.NodeName };

                    var matId = model.MaterialName;
                    var matName = GenerateFinalMaterialName(localPath, matId);
                    var foundMaterial = loadedMaterials.FirstOrDefault(x => x.Location == new UFile(matName, null));
                    if (foundMaterial != null)
                    {
                        var matReference = new AssetReference<MaterialAsset>(foundMaterial.Id, foundMaterial.Location);
                        meshParams.Material = matReference;
                    }
                    asset.MeshParameters.Add(model.MeshName, meshParams);
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
        [AssetDescription("Camera", "A camera")]
        class CameraAsset : Asset
        {

        }

        /// <summary>
        /// Used only for category purpose, there is no such thing as a Light asset (it will be a LightComponent inside an EntityAsset).
        /// </summary>
        [AssetDescription("Light", "A light")]
        class LightAsset : Asset
        {

        }
    }
}
