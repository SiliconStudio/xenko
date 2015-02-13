// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Paradox.Physics
{
    [Flags]
    public enum PhysicsEngineFlags
    {
        None = 0x0,

        CollisionsOnly = 0x1,

        SoftBodySupport = 0x2,

        MultiThreaded = 0x4,

        UseHardwareWhenPossible = 0x8,

        ContinuosCollisionDetection = 0x10
    }
}