// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;

namespace SiliconStudio.Xenko.Assets.Models
{
    public class SkeletonAssetCompiler : AssetCompilerBase
    {
        protected override void Prepare(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (SkeletonAsset)assetItem.Asset;
            var assetSource = GetAbsolutePath(assetItem, asset.Source);
            var extension = assetSource.GetFileExtension();
            var buildStep = new AssetBuildStep(assetItem);

            var importModelCommand = ImportModelCommand.Create(extension);
            if (importModelCommand == null)
            {
                result.Error($"No importer found for model extension '{extension}. The model '{assetSource}' can't be imported.");
                return;
            }

            importModelCommand.SourcePath = assetSource;
            importModelCommand.Location = targetUrlInStorage;
            importModelCommand.Mode = ImportModelCommand.ExportMode.Skeleton;
            importModelCommand.ScaleImport = asset.ScaleImport;
            importModelCommand.PivotPosition = asset.PivotPosition;
            importModelCommand.SkeletonNodesWithPreserveInfo = asset.NodesWithPreserveInfo;

            buildStep.Add(importModelCommand);

            result.BuildSteps = buildStep;
        }
    }
}
