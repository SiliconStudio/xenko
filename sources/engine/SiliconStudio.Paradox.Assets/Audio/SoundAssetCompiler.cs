// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Paradox.Assets.Audio
{
    public class SoundAssetCompiler : AssetCompilerBase<SoundAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, SoundAsset asset, AssetCompilerResult result)
        {
            if (!EnsureSourceExists(result, asset, assetAbsolutePath))
                return;

            // Get absolute path of asset source on disk
            var assetDirectory = assetAbsolutePath.GetParent();
            var assetSource = UPath.Combine(assetDirectory, asset.Source);

            result.BuildSteps = new AssetBuildStep(AssetItem) { new ImportStreamCommand
                {
                    DisableCompression = asset is SoundMusicAsset, // Media player need a not compressed file on Android and iOS
                    SourcePath = assetSource,
                    Location = urlInStorage,
                } };
        }
    }
}