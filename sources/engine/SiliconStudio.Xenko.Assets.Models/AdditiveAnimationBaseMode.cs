// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Models
{
    [DataContract]
    public enum AdditiveAnimationBaseMode
    {
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
