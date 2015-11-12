// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Animations;

namespace SiliconStudio.Xenko.Assets.Model
{
    [DataContract("Animation")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(AnimationAssetCompiler))]
    [ObjectFactory(typeof(AnimationFactory))]
    [Display(180, "Animation")]
    public class AnimationAsset : AssetImport
    {
        /// <summary>
        /// The default file extension used by the <see cref="AnimationAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkanim;.pdxanim";

        /// <summary>
        /// Gets or sets the scale import.
        /// </summary>
        /// <value>The scale import.</value>
        /// <userdoc>The scale factor to apply to the imported animation.</userdoc>
        [DataMember(10)]
        [DefaultValue(1.0f)]
        public float ScaleImport { get; set; }

        /// <summary>
        /// Gets or sets the animation repeat mode.
        /// </summary>
        /// <value>The repeat mode</value>
        /// <userdoc>Specifies how the animation should be played. That is played once and stop, infinitely loop, etc...</userdoc>
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