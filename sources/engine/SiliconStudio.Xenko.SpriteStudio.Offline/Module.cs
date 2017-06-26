// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Reflection;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Xenko.SpriteStudio.Offline
{
    public static class Module
    {
        [ModuleInitializer]
        public static void InitializeModule()
        {
            //RegisterPlugin(typeof(SpriteStudioPlugin));
            AssemblyRegistry.Register(typeof(Module).GetTypeInfo().Assembly, AssemblyCommonCategories.Assets);
        }
    }
}
