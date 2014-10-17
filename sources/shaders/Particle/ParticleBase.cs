// AUTO-GENERATED, DO NOT MODIFY!
using System;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Core.Mathematics;
using Buffer = SiliconStudio.Paradox.Graphics.Buffer;

namespace SiliconStudio.Paradox.Effects.Modules
{
    public static partial class ParticleBaseKeys
    {
        public static readonly ParameterKey<uint> ParticleCount = ParameterKeys.New<uint>();
        public static readonly ParameterKey<uint> ParticleStartIndex = ParameterKeys.New<uint>();
        public static readonly ParameterKey<Buffer> ParticleGlobalBuffer = ParameterKeys.New<Buffer>();
        public static readonly ParameterKey<Buffer> ParticleGlobalBufferRO = ParticleGlobalBuffer;
        public static readonly ParameterKey<Buffer> ParticleInputBuffer = ParameterKeys.New<Buffer>();
        public static readonly ParameterKey<Buffer> ParticleOutputBuffer = ParameterKeys.New<Buffer>();
        public static readonly ParameterKey<Buffer> ParticleSortBuffer = ParameterKeys.New<Buffer>();
        public static readonly ParameterKey<Buffer> ParticleSortBufferRO = ParticleSortBuffer;
    }
}
