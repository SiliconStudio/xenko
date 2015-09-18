// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.TextureConverter.Tests
{
    public static class Module
    {
        public const string ApplicationPath = "SiliconStudio.TextureConverter.Tests";
        public const string PathToInputImages = ApplicationPath+"/InputImages/";
        public const string PathToOutputImages = ApplicationPath+"SiliconStudio.TextureConverter.Tests/InputImages/";
        public const string PathToAtlasImages = PathToInputImages + "atlas/";
        
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