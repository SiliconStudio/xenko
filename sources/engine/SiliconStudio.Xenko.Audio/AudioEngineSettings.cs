// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Data;

namespace SiliconStudio.Xenko.Audio
{
    [DataContract]
    [Display("Audio")]
    public class AudioEngineSettings : Configuration
    {
        
        /// <userdoc>
        /// Enable HRTF audio. Note that only audio emitters with HRTF enabled produce HRTF audio
        /// </userdoc>
        [DataMember(10)]
        [Display("HRTF (if available)")]
        public bool HrtfSupport { get; set; }
    }
}
