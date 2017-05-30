// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Data;

namespace SiliconStudio.Xenko.Physics
{
    [DataContract]
    [Display("Physics")]
    public class PhysicsSettings : Configuration
    {

        [DataMember(10)]
        public PhysicsEngineFlags Flags;

        /// <userdoc>
        /// The maximum number of simulations the the physics engine can run in a frame to compensate for slowdown
        /// </userdoc>
        [DataMember(20)]
        public int MaxSubSteps = 1;

        /// <userdoc>
        /// The length in seconds of a physics simulation frame. The default is 0.016667 (one sixtieth of a second)
        /// </userdoc>
        [DataMember(30)]
        public float FixedTimeStep = 1.0f / 60.0f;
    }
}