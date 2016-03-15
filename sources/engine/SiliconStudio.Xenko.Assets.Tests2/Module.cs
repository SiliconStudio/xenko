// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS

using NUnit.Core.Extensibility;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Tests2
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
        }
    }
}

#endif