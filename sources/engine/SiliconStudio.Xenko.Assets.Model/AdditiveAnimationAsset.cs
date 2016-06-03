// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Xenko.Assets.Model
{
    [DataContract("AdditiveAnimation")]
    [Display(175, "Additive Animation")]
    public class AdditiveAnimationAsset : AnimationAsset
    {
        /// <summary>
        /// Gets or sets the path to the base source animation model when using additive animation.
        /// </summary>
        /// <value>The source.</value>
        /// <userdoc>The source file for the base animation.</userdoc>
        [DataMember(30)]
        [SourceFileMember(false)]
        public UFile BaseSource { get; set; } = new UFile("");

        /// <userdoc>Specifies how to use the base animation.</userdoc>
        [DataMember(40)]
        public AdditiveAnimationBaseMode Mode { get; set; }
    }

    [DataContract]
    public enum AdditiveAnimationBaseMode
    {
        // TODO: Add support for reference pose (need to add the concept to AnimationClip?)
        //ReferencePose = 0,

        /// <summary>
        /// Uses first frame of animation.
        /// </summary>
        FirstFrame = 1,

        /// <summary>
        /// Uses animation.
        /// </summary>
        Animation = 2,
    }
}
