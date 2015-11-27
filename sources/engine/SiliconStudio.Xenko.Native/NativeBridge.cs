// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.NativeBridge
{
    public static class NativeBridge
    {
#if SILICONSTUDIO_PLATFORM_IOS
        public const string Library = "__Internal";
#elif SILICONSTUDIO_PLATFORM_ANDROID
        public const string Library = "libSiliconStudio.Xenko.Native.so";
#else
        //Can't use this without .dll because our library has many dots apparently... http://stackoverflow.com/questions/9574974/dllimport-user32-vs-user32-dll
        public const string Library = "SiliconStudio.Xenko.Native.dll";
#endif
    }
}