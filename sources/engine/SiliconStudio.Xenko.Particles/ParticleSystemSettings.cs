// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
        /// Warm-up time is the amount of time the system should spend in background pre-simulation the first time it is started
        /// </summary>
        /// <userdoc>
        /// Warm-up time is the amount of time the system should spend in background pre-simulation the first time it is started (warming up). So when it is started it will appeas as if it has been running for some time laready
        /// </userdoc>
        [DataMember(10)]
        [Display("Warm-up time")]
        [DataMemberRange(0, 5, 0.01, 1)]
        [DefaultValue(0f)]
        public float WarmupTime { get; set; } = 0f;


    }
}
