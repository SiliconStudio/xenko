// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Sprite
{
    /// <summary>
    /// Describes a sprite group asset.
    /// </summary>
    [DataContract("SpriteGroup")]
    [AssetFileExtension(FileExtension)]
    [AssetCompiler(typeof(SpriteGroupCompiler))]
    [AssetFactory(typeof(SpriteGroupFactory))]
    [ThumbnailCompiler(PreviewerCompilerNames.SpriteGroupThumbnailCompilerQualifiedName)]
    [AssetDescription("Sprite Group", "A group of sprites", true)]
    public sealed class SpriteGroupAsset : ImageGroupAsset<SpriteInfo>
    {
        /// <summary>
        /// The default file extension used by the <see cref="SpriteGroupAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxsprite";
        
        private class SpriteGroupFactory : IAssetFactory
        {
            public Asset New()
            {
                return new SpriteGroupAsset();
            }
        }
    } 
}