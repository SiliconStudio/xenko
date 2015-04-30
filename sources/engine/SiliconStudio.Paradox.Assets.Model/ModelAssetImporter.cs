// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using System.Collections.Generic;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Assets.Entities;
using SiliconStudio.Paradox.Assets.Materials;
using SiliconStudio.Paradox.Rendering.Materials;
using SiliconStudio.Paradox.Assets.Textures;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Importer.Common;

namespace SiliconStudio.Paradox.Assets.Model
{
    public abstract class ModelAssetImporter : AssetImporterBase
    {
        private static readonly Type[] supportedTypes = { typeof(ModelAsset), typeof(TextureAsset), typeof(MaterialAsset), typeof(AnimationAsset) };

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
            var rawAssetReferences = new List<AssetItem>(); // the asset references without subdirectory path

            var entityInfo = GetEntityInfo(localPath, importParameters.Logger);

            //var isImportingEntity = importParameters.IsTypeSelectedForOutput<EntityAsset>();

            var isImportingModel = importParameters.IsTypeSelectedForOutput<ModelAsset>();

            var isImportingMaterial = importParameters.IsTypeSelectedForOutput<MaterialAsset>() ||
                                      isImportingModel;

            var isImportingTexture = importParameters.IsTypeSelectedForOutput<TextureAsset>() ||
                                     isImportingMaterial;

            // 1. Textures
            if (isImportingTexture)
            {
                ImportTextures(entityInfo.TextureDependencies, rawAssetReferences);
            }

            // 2. Animation
            if (importParameters.IsTypeSelectedForOutput<AnimationAsset>())
            {
                ImportAnimation(rawAssetReferences, localPath, entityInfo.AnimationNodes, isImportingModel);
            }

            // 3. Materials
            if (isImportingMaterial)
            {
                ImportMaterials(rawAssetReferences, entityInfo.Materials);
            }

            // 4. Model
            if (isImportingModel)
            {
                var modelItem = ImportModel(rawAssetReferences, localPath, localPath, entityInfo, false);

                // 5. Entity (currently disabled)
                //if (isImportingEntity)
                //{
                //    var entityAssetItem = ImportEntity(rawAssetReferences, localPath, modelItem);
                //
                //    // Apply EntityAnalysis 
                //    EntityAnalysis.UpdateEntityReferences(((EntityAsset)entityAssetItem.Asset).Hierarchy);
                //}
            }

            return rawAssetReferences;
        }

        private static Entity CreateTrackingEntity(EntityAsset entityAsset, Entity rootEntityAsset, ModelAsset modelAsset, string nodeName)
        {
            var childEntity = new Entity { Name = nodeName };

            // Add TransformComponent
            childEntity.Add(TransformComponent.Key, new TransformComponent());

            // Add ModelNodeLinkComponent
            childEntity.Add(ModelNodeLinkComponent.Key, new ModelNodeLinkComponent
            {
                NodeName = nodeName,
                Target = rootEntityAsset.Get(ModelComponent.Key),
            });

            // Add this asset to the list
            entityAsset.Hierarchy.Entities.Add(childEntity);

            // Get or create transformation component
            var transformationComponent = rootEntityAsset.GetOrCreate(TransformComponent.Key);

            // Mark node as preserved
            modelAsset.PreserveNodes(new List<string> { nodeName });

            // Add as children of model entity
            transformationComponent.Children.Add(childEntity.GetOrCreate(TransformComponent.Key));

            return childEntity;
        }

        private static AssetItem ImportEntity(List<AssetItem> assetReferences, UFile localPath, AssetItem modelItem)
        {
            var entityUrl = new UFile(localPath.GetFileName(), null);

            var asset = new EntityAsset { Source = localPath };
            var rootEntityData = new Entity();
            asset.Hierarchy.Entities.Add(rootEntityData);
            asset.Hierarchy.RootEntity = rootEntityData.Id;

            rootEntityData.Name = entityUrl;
            // Use modelUrl.Path to get the url without the extension
            rootEntityData.Add(ModelComponent.Key, new ModelComponent { Model = AttachedReferenceManager.CreateSerializableVersion<Rendering.Model>(modelItem.Id, modelItem.Location) });

            var assetReference = new AssetItem(entityUrl, asset);
            assetReferences.Add(assetReference);

            return assetReference;
        }

        private static void ImportAnimation(List<AssetItem> assetReferences, UFile localPath, List<string> animationNodes, bool shouldPostFixName)
        {
            if (animationNodes != null && animationNodes.Count > 0)
            {
                var assetSource = localPath;

                var asset = new AnimationAsset { Source = assetSource };
                var animUrl = localPath.GetFileName() + (shouldPostFixName? " Animation": "");

                assetReferences.Add(new AssetItem(animUrl, asset));
            }
        }

        private static AssetItem ImportModel(List<AssetItem> assetReferences, UFile assetSource, UFile localPath, EntityInfo entityInfo, bool shouldPostFixName)
        {
            var asset = new ModelAsset { Source = assetSource };

            if (entityInfo.Models != null)
            {
                var loadedMaterials = assetReferences.Where(x => x.Asset is MaterialAsset).ToList();
                foreach (var material in entityInfo.Materials)
                {
                    var foundMaterial = loadedMaterials.FirstOrDefault(x => x.Location == new UFile(material.Key, null));
                    if (foundMaterial != null)
                        asset.Materials.Add(new ModelMaterial { Name = material.Key, MaterialInstance = new MaterialInstance() { Material = AttachedReferenceManager.CreateSerializableVersion<Material>(foundMaterial.Id, foundMaterial.Location) } });
                }
            }

            if (entityInfo.Nodes != null)
            {
                foreach (var node in entityInfo.Nodes)
                    asset.Nodes.Add(new NodeInformation(node.Name, node.Depth, node.Preserve));
            }

            if (entityInfo.AnimationNodes != null && entityInfo.AnimationNodes.Count > 0)
                asset.PreserveNodes(entityInfo.AnimationNodes);

            var modelUrl = new UFile(localPath.GetFileName() + (shouldPostFixName?" Model": ""), null);
            var assetItem = new AssetItem(modelUrl, asset);
            assetReferences.Add(assetItem);
            return assetItem;
        }

        private static void ImportMaterials(List<AssetItem> assetReferences, Dictionary<string, MaterialAsset> materials)
        {
            if (materials != null)
            {
                var loadedTextures = assetReferences.Where(x => x.Asset is TextureAsset).ToList();

                foreach (var materialKeyValue in materials)
                {
                    AdjustForTransparency(materialKeyValue.Value);
                    var material = materialKeyValue.Value;

                    // patch texture name and ids
                    var materialAssetReferences = AssetReferenceAnalysis.Visit(material);
                    foreach (var materialAssetReferenceLink in materialAssetReferences)
                    {
                        var materialAssetReference = materialAssetReferenceLink.Reference as IContentReference;
                        if (materialAssetReference == null)
                            continue;

                        // texture location is #nameOfTheModel_#nameOfTheTexture at this point in the material
                        var foundTexture = loadedTextures.FirstOrDefault(x => x.Location == materialAssetReference.Location);
                        if (foundTexture != null)
                        {
                            materialAssetReferenceLink.UpdateReference(foundTexture.Id, foundTexture.Location);
                        }
                    }

                    var assetReference = new AssetItem(materialKeyValue.Key, material);
                    assetReferences.Add(assetReference);
                }
            }
        }

        /// <summary>
        /// Modify the material to comply with its transparency parameters.
        /// </summary>
        /// <param name="material">The material/</param>
        private static void AdjustForTransparency(MaterialAsset material)
        {
            //// Note: at this point, there is no other nodes than diffuse, specular, transparent, normal and displacement
            //if (material.ColorNodes.ContainsKey(MaterialParameters.AlbedoDiffuse))
            //{
            //    var diffuseNode = material.GetMaterialNode(MaterialParameters.AlbedoDiffuse);
            //    if (material.ColorNodes.ContainsKey(MaterialParameters.TransparencyMap))
            //    {
            //        var diffuseNodeName = material.ColorNodes[MaterialParameters.AlbedoDiffuse];
            //        var transparentNodeName = material.ColorNodes[MaterialParameters.TransparencyMap];

            //        var transparentNode = material.GetMaterialNode(MaterialParameters.TransparencyMap);

            //        if (diffuseNode == null || transparentNode == null)
            //            return;

            //        var foundTextureDiffuse = FindTextureNode(material, diffuseNodeName);
            //        var foundTextureTransparent = FindTextureNode(material, transparentNodeName);

            //        if (foundTextureDiffuse != null && foundTextureTransparent != null)
            //        {
            //            if (foundTextureDiffuse != foundTextureTransparent)
            //            {
            //                var alphaMixNode = new MaterialBinaryComputeNode(diffuseNode, transparentNode, BinaryOperator.SubstituteAlpha);
            //                material.AddColorNode(MaterialParameters.AlbedoDiffuse, "pdx_diffuseWithAlpha", alphaMixNode);
            //            }
            //        }

            //        // set the key if it was missing
            //        material.Parameters.Set(MaterialParameters.HasTransparency, true);
            //    }
            //    else
            //    {
            //        // NOTE: MaterialParameters.HasTransparency is mostly runtime
            //        var isTransparent = false;
            //        if (material.Parameters.ContainsKey(MaterialParameters.HasTransparency))
            //            isTransparent = (bool)material.Parameters[MaterialParameters.HasTransparency];
                    
            //        if (!isTransparent)
            //        {
            //            // remove the diffuse node
            //            var diffuseName = material.ColorNodes[MaterialParameters.AlbedoDiffuse];
            //            material.Nodes.Remove(diffuseName);

            //            // add the new one
            //            var opaqueNode = new MaterialBinaryComputeNode(diffuseNode, null, BinaryOperator.Opaque);
            //            material.AddColorNode(MaterialParameters.AlbedoDiffuse, "pdx_diffuseOpaque", opaqueNode);
            //        }
            //    }
            //}
        }

        private static void ImportTextures(IEnumerable<string> textureDependencies, List<AssetItem> assetReferences)
        {
            if (textureDependencies == null)
                return;

            foreach (var textureFullPath in textureDependencies.Distinct(x => x))
            {
                var texturePath = new UFile(textureFullPath);

                var source = texturePath;
                var texture = new TextureAsset { Source = source, PremultiplyAlpha = false };

                // Create asset reference
                assetReferences.Add(new AssetItem(texturePath.GetFileName(), texture));
            }
        }
    }
}
