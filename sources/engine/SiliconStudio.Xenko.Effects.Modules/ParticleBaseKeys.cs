// Copyright (c) 2011 Silicon Studio

using Paradox.Framework.Graphics;

namespace Paradox.Effects.Modules
{
    /// <summary>
    /// Keys used for the particle system.
    /// </summary>
    public static partial class ParticleBaseKeys
    {
        static ParticleBaseKeys()
        {
            ParticleGlobalBufferRO = ParticleGlobalBuffer;
            ParticleSortBufferRO = ParticleSortBuffer;
        }
    }
}