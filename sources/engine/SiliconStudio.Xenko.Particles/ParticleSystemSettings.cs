// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Particles
{
    [DataContract("ParticleSystemSettings")]
    [Display("Settings")]
    public class ParticleSystemSettings
    {
        /// <summary>
        /// Warm-up time is the amount of time the system should spend in the background pre-simulation the first time it is started
        /// </summary>
        /// <userdoc>
        /// Warm-up time is the amount of time the system should spend in the background pre-simulation the first time it is started (warming up). So when it is started it will appear as if it has been running for some time already
        /// </userdoc>
        [DataMember(10)]
        [Display("Warm-up time")]
        [DataMemberRange(0, 5, 0.01, 1, 3)]
        [DefaultValue(0f)]
        public float WarmupTime { get; set; } = 0f;


    }
}
