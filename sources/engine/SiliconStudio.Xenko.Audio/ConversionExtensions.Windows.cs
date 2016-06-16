// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS && !SILICONSTUDIO_XENKO_SOUND_SDL

using SharpDX.Mathematics.Interop;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Audio
{
    internal static class ConversionExtensions
    {
        public static unsafe RawVector3 ToSharpDx(this Vector3 vec)
        {
            return *((RawVector3*)&vec);
        }
    }
}

#endif
