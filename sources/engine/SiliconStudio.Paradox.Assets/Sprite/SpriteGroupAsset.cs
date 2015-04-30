// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Paradox.Assets.Sprite
{
    /// <summary>
    /// Describes a sprite group asset.
    /// </summary>
    [DataContract("SpriteGroup")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(SpriteGroupCompiler))]
    [ObjectFactory(typeof(SpriteGroupFactory))]
    [ThumbnailCompiler(PreviewerCompilerNames.SpriteGroupThumbnailCompilerQualifiedName, true)]
    [Display(160, "Sprite Group", "A group of sprites")]
    public sealed class SpriteGroupAsset : ImageGroupAsset<SpriteInfo>
    {
        /// <summary>
        /// The default file extension used by the <see cref="SpriteGroupAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxsprite";
        
        private class SpriteGroupFactory : IObjectFactory
        {
            public object New(Type type)
            {
                return new SpriteGroupAsset();
            }
        }
    } 
}