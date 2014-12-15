// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Paradox.DataModel;

namespace SiliconStudio.Paradox.Assets.Model
{
    [DataContract("Animation")]
    [AssetFileExtension(FileExtension)]
    [AssetCompiler(typeof(AnimationAssetCompiler))]
    [AssetFactory(typeof(AnimationFactory))]
    [ThumbnailCompiler(PreviewerCompilerNames.AnimationThumbnailCompilerQualifiedName)]
    [Display("Animation", "A skeletal animation")]
    public class AnimationAsset : AssetImport
    {
        /// <summary>
        /// The default file extension used by the <see cref="AnimationAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxanim";

        /// <summary>
        /// Gets or sets the animation repeat mode.
        /// </summary>
        /// <value>The repeat mode</value>
        [DataMember(20)]
        public AnimationRepeatMode RepeatMode { get; set; }

        /// <summary>
        /// Create an instance of <see cref="AnimationAsset"/> with default values.
        /// </summary>
        public AnimationAsset()
        {
            RepeatMode = AnimationRepeatMode.LoopInfinite;
        }

        private class AnimationFactory : IAssetFactory
        {
            public Asset New()
            {
                return new AnimationAsset();
            }
        }
    }
}