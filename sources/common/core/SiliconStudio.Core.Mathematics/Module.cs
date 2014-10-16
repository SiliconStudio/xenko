// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under MIT License. See LICENSE.md for details.
using System.Reflection;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Mathematics
{
    internal class Module
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            AssemblyRegistry.Register(typeof(Module).GetTypeInfo().Assembly, AssemblyCommonCategories.Assets);
        }
    }
}
