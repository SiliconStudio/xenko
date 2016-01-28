// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Physics
{
    public interface IPhysicsSystem : IGameSystemBase
    {
        Simulation Create(PhysicsProcessor processor, PhysicsEngineFlags flags = PhysicsEngineFlags.None);
        void Release(PhysicsProcessor processor);
    }
}
