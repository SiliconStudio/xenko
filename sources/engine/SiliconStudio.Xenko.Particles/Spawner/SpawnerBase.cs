// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles.Spawner
{
    [DataContract("SpawnerBase")]
    public abstract class SpawnerBase
    {
        public abstract void SpawnNew(float dt, ParticlePool pool);

        public abstract void RemoveOld(float dt, ParticlePool pool);

        public abstract int GetMaxParticles();
    }
}
