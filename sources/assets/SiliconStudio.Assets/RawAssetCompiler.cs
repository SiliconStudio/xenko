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
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, AssetItem assetItem, RawAsset asset, AssetCompilerResult result)
        {
            // Get absolute path of asset source on disk
            var assetSource = GetAbsolutePath(assetAbsolutePath, asset.Source);
            var importCommand = new ImportStreamCommand(urlInStorage, assetSource) { DisableCompression = !asset.Compress };

            result.BuildSteps = new AssetBuildStep(assetItem) { importCommand };
        }
    }
}
