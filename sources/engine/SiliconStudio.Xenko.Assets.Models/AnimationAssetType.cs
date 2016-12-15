// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Xenko.Animations;

namespace SiliconStudio.Xenko.Assets.Models
{
    [DataContract("AnimationAssetType")]

    public abstract class AnimationAssetType
    {
        [DataMemberIgnore]
        public abstract AnimationClipBlendMode BlendMode { get; }
    }

    [Display("Animation Clip")]
    [DataContract("StandardAnimationAssetType")]

    public class StandardAnimationAssetType : AnimationAssetType
    {
        [DataMemberIgnore]
        public override AnimationClipBlendMode BlendMode => AnimationClipBlendMode.LinearBlend;
    }

    [Display("Difference Clip")]
    [DataContract("DifferenceAnimationAssetType")]

    public class DifferenceAnimationAssetType : AnimationAssetType
    {
        [DataMemberIgnore]
        public override AnimationClipBlendMode BlendMode => AnimationClipBlendMode.Additive;

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

        /// <userdoc>Specifies how to use the base animation.</userdoc>
        [DataMember(40)]
        public AdditiveAnimationBaseMode Mode { get; set; } = AdditiveAnimationBaseMode.Animation;
    }
}
