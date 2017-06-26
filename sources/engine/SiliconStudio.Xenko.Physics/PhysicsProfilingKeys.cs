// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.Physics
{
    public static class PhysicsProfilingKeys
    {
        public static ProfilingKey SimulationProfilingKey = new ProfilingKey("Physics Simulation");

        public static ProfilingKey ContactsProfilingKey = new ProfilingKey("Physics Contacts");

        public static ProfilingKey CharactersProfilingKey = new ProfilingKey(SimulationProfilingKey, "Physics Characters");
    }
}
