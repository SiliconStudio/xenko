// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Data;

namespace SiliconStudio.Xenko.Audio
{
    [DataContract]
    [Display("Audio Settings")]
    public class AudioEngineSettings : Configuration
    {
        [DataMember(10)]
        [Display("HRTF Support (If Available)")]
        public bool HrtfSupport { get; set; }
    }
}
