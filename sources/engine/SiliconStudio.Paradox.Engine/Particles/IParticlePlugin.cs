// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Particles
{
    /// <summary>
    /// A particle plugin that can updates or process particles.
    /// </summary>
    public interface IParticlePlugin
    {
        void Update(ParticleSystem particleSystem, float dt);
    }
}