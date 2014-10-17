// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Audio
{
    [DataContract("SoundEffect")]
    [AssetFileExtension(FileExtension)]
    [AssetFactory(typeof(SoundEffectFactory))]
    [AssetCompiler(typeof(SoundAssetCompiler))]
    [ThumbnailCompiler(PreviewerCompilerNames.SoundThumbnailCompilerQualifiedName)]
    [AssetDescription("Sound Effect", "A sound effect", false)]
    public class SoundEffectAsset : SoundAsset
    {
        private class SoundEffectFactory : IAssetFactory
        {
            public Asset New()
            {
                return new SoundEffectAsset();
            }
        }
    }
}