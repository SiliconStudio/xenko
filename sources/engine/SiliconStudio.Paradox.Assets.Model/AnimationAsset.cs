// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Animations;

namespace SiliconStudio.Paradox.Assets.Model
{
    [DataContract("Animation")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(AnimationAssetCompiler))]
    [ObjectFactory(typeof(AnimationFactory))]
    [ThumbnailCompiler(PreviewerCompilerNames.AnimationThumbnailCompilerQualifiedName)]
    [Display(180, "Animation", "A skeletal animation")]
    public class AnimationAsset : AssetImport
    {
        /// <summary>
        /// The default file extension used by the <see cref="AnimationAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxanim";

        /// <summary>
        /// Gets or sets the scale import.
        /// </summary>
        /// <value>The scale import.</value>
        [DataMember(10)]
        [DefaultValue(1.0f)]
        public float ScaleImport { get; set; }

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
            ScaleImport = 1.0f;
        }

        private class AnimationFactory : IObjectFactory
        {
            public object New(Type type)
            {
                return new AnimationAsset();
            }
        }
    }
}