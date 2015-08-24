// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Paradox.Assets.Model
{
    internal class Module
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            AssemblyRegistry.Register(typeof(Module).Assembly, AssemblyCommonCategories.Assets);

            // Preload these dlls
            NativeLibrary.PreloadLibrary("libfbxsdk.dll");
            NativeLibrary.PreloadLibrary("assimp-vc120-mt.dll");
        }
    }
}
