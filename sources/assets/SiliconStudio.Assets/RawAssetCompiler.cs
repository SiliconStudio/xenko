// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Raw asset compiler.
    /// </summary>
    internal class RawAssetCompiler : AssetCompilerBase<RawAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, RawAsset asset, AssetCompilerResult result)
        {
            if (!EnsureSourcesExist(result, asset, assetAbsolutePath))
                return;
        
            // Get absolute path of asset source on disk
            var assetSource = GetAbsolutePath(assetAbsolutePath, asset.Source);
            var importCommand = new ImportStreamCommand(urlInStorage, assetSource) { DisableCompression = !asset.Compress };

            result.BuildSteps = new AssetBuildStep(AssetItem) { importCommand };
        }
    }
}
