// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Audio
{
    [DataContract("SoundMusic")]
    [AssetFileExtension(FileExtension)]
    [AssetFactory(typeof(SoundMusicFactory))]
    [AssetCompiler(typeof(SoundAssetCompiler))]
    [ThumbnailCompiler(PreviewerCompilerNames.SoundThumbnailCompilerQualifiedName)]
    [AssetDescription("Sound Music", "A music track", false)]
    public class SoundMusicAsset : SoundAsset
    {
        private class SoundMusicFactory : IAssetFactory
        {
            public Asset New()
            {
                return new SoundMusicAsset();
            }
        }
    }
}