// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using SiliconStudio.Core;

namespace SiliconStudio.TextureConverter.Tests
{
    public static class Module
    {
        public static readonly string ApplicationPath = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string PathToInputImages = Path.Combine(ApplicationPath, "InputImages") + Path.DirectorySeparatorChar;
        public static readonly string PathToOutputImages = Path.Combine(ApplicationPath, "OutputImages") + Path.DirectorySeparatorChar;
        public static readonly string PathToAtlasImages = PathToInputImages + "atlas" + Path.DirectorySeparatorChar;
        
        static Module()
        {
            LoadLibraries();
        }

        public static void LoadLibraries()
        {
            NativeLibrary.PreloadLibrary("AtitcWrapper.dll");
            NativeLibrary.PreloadLibrary("DxtWrapper.dll");
            NativeLibrary.PreloadLibrary("PVRTexLib.dll");
            NativeLibrary.PreloadLibrary("PvrttWrapper.dll");
            NativeLibrary.PreloadLibrary("FreeImage.dll");
            NativeLibrary.PreloadLibrary("FreeImageNET.dll");
        }
    }
}