// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Physics
{
    public interface IPhysicsSystem : IGameSystemBase
    {
        Simulation Create(PhysicsProcessor processor, PhysicsEngineFlags flags = PhysicsEngineFlags.None);
        void Release(PhysicsProcessor processor);
    }
}
