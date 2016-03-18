// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS && !SILICONSTUDIO_XENKO_SOUND_SDL

using SharpDX.Mathematics.Interop;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Audio
{
    internal static class ConversionExtensions
    {
        public static unsafe RawVector3 ToSharpDX(this Vector3 vec)
        {
            return *((RawVector3*)&vec);
        }

        public static SharpDX.Multimedia.WaveFormat ToSharpDX(this Wave.WaveFormat format)
        {
            return new SharpDX.Multimedia.WaveFormat(format.SampleRate, format.BitsPerSample, format.Channels);
        }
    }
}

#endif