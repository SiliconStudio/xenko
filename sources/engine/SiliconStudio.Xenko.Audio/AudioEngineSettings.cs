// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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