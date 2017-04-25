// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Assets.Materials;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets.Models
{
    [AssetCompiler(typeof(ModelAsset), typeof(AssetCompilationContext))]
    public class ModelAssetCompiler : AssetCompilerBase
    {
        public override IEnumerable<KeyValuePair<Type, BuildDependencyType>> GetInputTypes(AssetCompilerContext context, AssetItem assetItem)
        {
            yield return new KeyValuePair<Type, BuildDependencyType>(typeof(SkeletonAsset), BuildDependencyType.Runtime | BuildDependencyType.CompileContent);
            yield return new KeyValuePair<Type, BuildDependencyType>(typeof(MaterialAsset), BuildDependencyType.Runtime);
        }

        public override IEnumerable<ObjectUrl> GetInputFiles(AssetCompilerContext context, AssetItem assetItem)
        {
            var modelAsset = (ModelAsset)assetItem.Asset;

            if (modelAsset.Skeleton != null)
            {
                var skeleton = assetItem.Package.FindAssetFromProxyObject(modelAsset.Skeleton);
                if (skeleton != null)
                {
                    yield return new ObjectUrl(UrlType.Content, skeleton.Location);
                }
            }
            
            var assetDirectory = assetItem.FullPath.GetParent();
            var assetSource = UPath.Combine(assetDirectory, modelAsset.Source);
            yield return new ObjectUrl(UrlType.File, assetSource);
        }

        protected override void Prepare(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (ModelAsset)assetItem.Asset;
            // Get absolute path of asset source on disk
            var assetDirectory = assetItem.FullPath.GetParent();
            var assetSource = UPath.Combine(assetDirectory, asset.Source);

            var gameSettingsAsset = context.GetGameSettingsAsset();
            var renderingSettings = gameSettingsAsset.GetOrCreate<RenderingSettings>();
            var allow32BitIndex = renderingSettings.DefaultGraphicsProfile >= GraphicsProfile.Level_9_2;
            var maxInputSlots = renderingSettings.DefaultGraphicsProfile >= GraphicsProfile.Level_10_1 ? 32 : 16;
            var allowUnsignedBlendIndices = context.GetGraphicsPlatform(assetItem.Package) != GraphicsPlatform.OpenGLES;
            var extension = asset.Source.GetFileExtension();

            // Find skeleton asset, if any
            AssetItem skeleton = null;
            if (asset.Skeleton != null)
                skeleton = assetItem.Package.FindAssetFromProxyObject(asset.Skeleton);

            var importModelCommand = ImportModelCommand.Create(extension);
            if (importModelCommand == null)
            {
                result.Error($"No importer found for model extension '{extension}. The model '{assetSource}' can't be imported.");
                return;
            }

            importModelCommand.InputFilesGetter = () => GetInputFiles(context, assetItem);
            importModelCommand.Mode = ImportModelCommand.ExportMode.Model;
            importModelCommand.SourcePath = assetSource;
            importModelCommand.Location = targetUrlInStorage;
            importModelCommand.Allow32BitIndex = allow32BitIndex;
            importModelCommand.MaxInputSlots = maxInputSlots;
            importModelCommand.AllowUnsignedBlendIndices = allowUnsignedBlendIndices;
            importModelCommand.Materials = asset.Materials;
            importModelCommand.ScaleImport = asset.ScaleImport;
            importModelCommand.PivotPosition = asset.PivotPosition;
            importModelCommand.SkeletonUrl = skeleton?.Location;
            importModelCommand.Package = assetItem.Package;

            result.BuildSteps = new AssetBuildStep(assetItem) { importModelCommand };
        }
    }
}
