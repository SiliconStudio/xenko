// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Runtime.CompilerServices;
using NUnit.Core.Extensibility;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core;

namespace SiliconStudio.Assets.Tests
{
    // Somehow it helps Resharper NUnit to run module initializer first (to determine unit test configuration).
    [NUnitAddin]
    public class Module : IAddin
    {
        public bool Install(IExtensionHost host)
        {
            return false;
        }

        [ModuleInitializer]
        internal static void Initialize()
        {
            if (!PlatformFolders.IsVirtualFileSystemInitialized)
                PlatformFolders.ApplicationDataSubDirectory = typeof(Module).Assembly.GetName().Name;
            AssemblyRegistry.Register(typeof(Module).Assembly, AssemblyCommonCategories.Assets);
            RuntimeHelpers.RunModuleConstructor(typeof(Asset).Module.ModuleHandle);
        }
    }
}