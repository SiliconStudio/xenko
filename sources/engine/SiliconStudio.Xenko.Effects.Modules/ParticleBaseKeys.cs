// Copyright (c) 2011 Silicon Studio

using Xenko.Framework.Graphics;

namespace Xenko.Effects.Modules
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