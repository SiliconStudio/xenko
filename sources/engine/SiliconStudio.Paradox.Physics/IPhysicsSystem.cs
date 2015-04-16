// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Physics
{
    public interface IPhysicsSystem : IGameSystemBase
    {
        Simulation Create(PhysicsProcessor processor, PhysicsEngineFlags flags = PhysicsEngineFlags.None);
        void Release(PhysicsProcessor processor);
    }
}
