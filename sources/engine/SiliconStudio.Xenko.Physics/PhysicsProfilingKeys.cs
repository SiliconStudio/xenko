// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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