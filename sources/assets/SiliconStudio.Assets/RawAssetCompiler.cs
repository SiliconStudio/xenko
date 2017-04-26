// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Assets.Compiler;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Raw asset compiler.
    /// </summary>
    [AssetCompiler(typeof(RawAsset), typeof(AssetCompilationContext))]
    internal class RawAssetCompiler : AssetCompilerBase
    {
        protected override void Prepare(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (RawAsset)assetItem.Asset;

            // Get absolute path of asset source on disk
            var assetSource = GetAbsolutePath(assetItem, asset.Source);
            var importCommand = new ImportStreamCommand(targetUrlInStorage, assetSource) { DisableCompression = !asset.Compress };

            result.BuildSteps = new AssetBuildStep(assetItem) { importCommand };
        }
    }
}
