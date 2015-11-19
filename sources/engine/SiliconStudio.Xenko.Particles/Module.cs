// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Reflection;
using System.Runtime.CompilerServices;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Particles.Spawner;

namespace SiliconStudio.Xenko.Particles
{
    internal class Module
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            AssemblyRegistry.Register(typeof(Module).GetTypeInfo().Assembly, AssemblyCommonCategories.Assets);
            RuntimeHelpers.RunModuleConstructor(typeof(SpawnerBase).Module.ModuleHandle);
        }
    }
}
