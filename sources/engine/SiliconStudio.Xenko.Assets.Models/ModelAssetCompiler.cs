// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core.IO;
using SiliconStudio.Xenko.Assets.Materials;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets.Models
{
    public class ModelAssetCompiler : AssetCompilerBase
    {
        public ModelAssetCompiler()
        {
            CompileTimeDependencyTypes.Add(typeof(SkeletonAsset));
            CompileTimeDependencyTypes.Add(typeof(MaterialAsset));
        }

        protected override void Compile(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (ModelAsset)assetItem.Asset;
            // Get absolute path of asset source on disk
            var assetDirectory = assetItem.FullPath.GetParent();
            var assetSource = UPath.Combine(assetDirectory, asset.Source);

            var gameSettingsAsset = context.GetGameSettingsAsset();
            var renderingSettings = gameSettingsAsset.Get<RenderingSettings>();
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
