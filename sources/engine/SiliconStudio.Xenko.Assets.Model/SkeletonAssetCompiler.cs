// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Xenko.Assets.Model
{
    public class SkeletonAssetCompiler : AssetCompilerBase<SkeletonAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, SkeletonAsset asset, AssetCompilerResult result)
        {
            if (!EnsureSourcesExist(result, asset, assetAbsolutePath))
                return;

            var assetSource = GetAbsolutePath(assetAbsolutePath, asset.Source);
            var extension = assetSource.GetFileExtension();
            var buildStep = new AssetBuildStep(AssetItem);

            var importModelCommand = ImportModelCommand.Create(extension);
            if (importModelCommand == null)
            {
                result.Error("No importer found for model extension '{0}. The model '{1}' can't be imported.", extension, assetSource);
                return;
            }

            importModelCommand.SourcePath = assetSource;
            importModelCommand.Location = urlInStorage;
            importModelCommand.Mode = ImportModelCommand.ExportMode.Skeleton;
            importModelCommand.ScaleImport = asset.ScaleImport;
            importModelCommand.SkeletonNodesWithPreserveInfo = asset.NodesWithPreserveInfo;

            buildStep.Add(importModelCommand);

            result.BuildSteps = buildStep;
        }
    }
}
