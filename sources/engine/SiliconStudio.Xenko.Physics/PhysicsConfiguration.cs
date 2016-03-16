// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Data;

namespace SiliconStudio.Xenko.Physics
{
    [DataContract]
    [Display("Physics Settings")]
    public class PhysicsSettings : Configuration
    {
        [DataMember(10)]
        public PhysicsEngineFlags Flags;

        [DataMember(20)]
        public int MaxSubSteps = 1;

        [DataMember(30)]
        public float FixedTimeStep = 1.0f / 60.0f;
    }
}