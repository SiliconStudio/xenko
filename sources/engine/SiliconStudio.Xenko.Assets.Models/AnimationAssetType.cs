// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Xenko.Assets.Models
{
    [DataContract("AnimationAssetType")]
    public abstract class AnimationAssetType
    {
        [DataMemberIgnore]
        public abstract AnimationAssetTypeEnum Type { get; }
    }

    [Display("Animation Clip")]
    [DataContract("StandardAnimationAssetType")]
    public class StandardAnimationAssetType : AnimationAssetType
    {
        [DataMemberIgnore]
        public override AnimationAssetTypeEnum Type => AnimationAssetTypeEnum.AnimationClip;
    }

    [Display("Difference Clip")]
    [DataContract("DifferenceAnimationAssetType")]
    public class DifferenceAnimationAssetType : AnimationAssetType
    {
        [DataMemberIgnore]
        public override AnimationAssetTypeEnum Type => AnimationAssetTypeEnum.DifferenceClip;

        /// <summary>
        /// Gets or sets the path to the base source animation model when using additive animation.
        /// </summary>
        /// <value>The path to the reference clip.</value>
        /// <userdoc>
        /// The reference clip (R) is what the difference clip (D) will be calculated against, effectively resulting in D = S - R ((S) being the source clip)
        /// </userdoc>
        [DataMember(30)]
        [SourceFileMember(false)]
        [Display("Reference")]
        public UFile BaseSource { get; set; } = new UFile("");

        #region Animation frames
        // Please note that animation frames are edited using the AnimationFrameTemplateProvider
        //  All 3 properties are required with the exact same names 
        //  AnimationFrameMinimum should be set to match the actual first frame of the animation 
        //  AnimationFrameMaximum should be set to match the actual length of the animation frame sequence

        /// <summary>
        /// Gets or sets the start frame of the animation.
        /// </summary>
        [DataMember(32)]
        [Display("Start frame")]
        [DefaultValue(0)]
        public long StartAnimationFrame { get; set; } = 0;

        /// <summary>
        /// Gets or sets the end frame of the animation.
        /// </summary>
        [DataMember(34)]
        [Display("End frame")]
        [DefaultValue(1000)]
        public long EndAnimationFrame { get; set; } = 1000;

        // TODO Hide it later
        [DefaultValue(0)]
        public long AnimationFrameMinimum { get; set; } = 0;

        // TODO Hide it later
        [DefaultValue(1000)]
        public long AnimationFrameMaximum { get; set; } = 1000;

        #endregion

        /// <userdoc>Specifies how to use the base animation.</userdoc>
        [DataMember(40)]
        public AdditiveAnimationBaseMode Mode { get; set; } = AdditiveAnimationBaseMode.Animation;
    }

    /// <summary>
    /// Type which describes the nature of the animation clip we want to use.
    /// The terms are borrowed from the book Game Engine Architecture, Chapter 11.6.5 Additive Blending
    /// </summary>
    [DataContract]
    public enum AnimationAssetTypeEnum
    {
        /// <summary>
        /// Single source animation clip which animates the character.
        /// </summary>
        /// <userdoc>
        /// Single source animation clip which animates the character.
        /// </userdoc>
        [Display("Animation Clip")]
        AnimationClip = 1,

        /// <summary>
        /// Difference animation clip is computed as the difference against another animation. It is usually used for additive blending.
        /// </summary>
        /// <userdoc>
        /// Difference animation clip is computed as the difference against another animation. It is usually used for additive blending.
        /// </userdoc>
        [Display("Difference Clip")]
        DifferenceClip = 2,
    }
}
