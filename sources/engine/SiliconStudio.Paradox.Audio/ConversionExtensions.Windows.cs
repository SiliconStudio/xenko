// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Audio
{
    internal static class ConversionExtensions
    {
        public static SharpDX.Vector3 ToSharpDX(this Vector3 vec)
        {
            return new SharpDX.Vector3(vec.X, vec.Y, vec.Z);
        }

        public static SharpDX.Multimedia.WaveFormat ToSharpDX(this Wave.WaveFormat format)
        {
            return new SharpDX.Multimedia.WaveFormat(format.SampleRate, format.BitsPerSample, format.Channels);
        }
    }
}

#endif