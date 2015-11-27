// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.NativeBridge
{
    public static class Module
    {
        [ModuleInitializer]
        public static void InitializeModule()
        {
            NativeLibrary.PreloadLibrary("SiliconStudio.Xenko.Native.dll");
        }
    }
}